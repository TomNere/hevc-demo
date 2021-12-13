using HEVCDemo.Hevc;
using HEVCDemo.Models;
using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Parsers
{
    public class CodingPredictionUnitsParser
    {
        private readonly VideoSequence videoSequence;

        public CodingPredictionUnitsParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(VideoCache cacheProvider)
        {
            return await Task.Run(() =>
            {
                if (videoSequence.Width == 0 || videoSequence.MaxCUSize == 0)
                {
                    throw new FormatException("InvalidHeightWidthEx,Text".Localize());
                }

                int CUOneRow = (videoSequence.Width + videoSequence.MaxCUSize - 1) / videoSequence.MaxCUSize;
                int iLCUSize = videoSequence.MaxCUSize;

                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.CodingPredictionUnitsFilePath);
                    string strOneLine = file.ReadLine();
                    int decOrder = -1;
                    int lastPOC = -1;

                    /// <1,1> 99 0 0 5 0
                    while (strOneLine != null)
                    {
                        if (strOneLine[0] != '<')
                        {
                            throw new FormatException("Line must start with <");
                        }

                        int frameNumber = int.Parse(strOneLine.Substring(1, strOneLine.LastIndexOf(',') - 1));

                        while (true)
                        {
                            int pocStart = strOneLine.LastIndexOf('<');
                            int addressStart = strOneLine.LastIndexOf(',');
                            int addressEnd = strOneLine.LastIndexOf('>');
                            int poc = int.Parse(strOneLine.Substring(pocStart + 1, addressStart - pocStart - 1));
                            int address = int.Parse(strOneLine.Substring(addressStart + 1, addressEnd - addressStart - 1));

                            decOrder += lastPOC != poc ? 1 : 0;
                            ComFrame frame;

                            if (poc != lastPOC)
                            {
                                frame = new ComFrame { Poc = poc, Sequence = videoSequence };
                                videoSequence.FramesInDecodeOrder.Add(decOrder, frame);
                            }
                            else
                            {
                                frame = videoSequence.FramesInDecodeOrder[decOrder];
                            }

                            lastPOC = poc;

                            var tokens = strOneLine.Substring(addressEnd + 2).Split(' ');

                            /// Poc and Lcu addr
                            var pcLCU = new CodingUnit { Address = address, Frame = frame, Size = iLCUSize };
                            pcLCU.PixelX = (address % CUOneRow) * videoSequence.MaxCUSize;
                            pcLCU.PixelY = (address / CUOneRow) * videoSequence.MaxCUSize;

                            /// Recursively parse the CU&PU quard-tree structure
                            var index = 0;
                            if (!ReadInCUMode(tokens, pcLCU, ref index))
                            {
                                throw new FormatException("Invalid format");
                            }

                            frame.CodingUnits.Add(pcLCU);

                            strOneLine = file.ReadLine();
                            if (strOneLine == null || int.Parse(strOneLine.Substring(1, strOneLine.LastIndexOf(',') - 1)) != frameNumber)
                            {
                                break;
                            }
                        }
                    }
                    file.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{"ErrorParsingEx,Text".Localize()}\n\n{e.Message}", "AppTitle,Title".Localize());
                    return false;
                }

                return true;
            });
        }

        public bool ReadInCUMode(string[] tokens, CodingUnit pcLCU, ref int index)
        {
            if (index > tokens.Length - 1) return false;

            var CUMode = int.Parse(tokens[index]);

            if (CUMode == 99)
            {
                int maxDepth = pcLCU.Frame.Sequence.MaxCUDepth;
                int totalNumPart = 1 << ((maxDepth - pcLCU.Depth) << 1);

                /// Non-leaf node, add 4 children CUs
                for (int i = 0; i < 4; i++)
                {
                    var pcChildNode = new CodingUnit 
                    {
                        Frame = pcLCU.Frame,
                        Address = pcLCU.Address, 
                        Size = pcLCU.Size / 2,
                        Depth = pcLCU.Depth + 1,
                        ZOrder = pcLCU.ZOrder + (totalNumPart/4) * i
                    };

                    int subCUX = pcLCU.PixelX + i % 2 * (pcLCU.Size / 2);
                    int subCUY = pcLCU.PixelY + i / 2 * (pcLCU.Size / 2);
                    pcChildNode.PixelX = subCUX;
                    pcChildNode.PixelY = subCUY;
                    pcLCU.SubCUs.Add(pcChildNode);
                    index++;
                    ReadInCUMode(tokens, pcChildNode, ref index);
                }
            }
            else
            {
                /// Leaf node - create PUs and write the PU Mode for it
                pcLCU.PartitionSize = (PartitionSize)CUMode;

                int iPUCount = GetPUNumber((PartitionSize)CUMode);
                for (int i = 0; i < iPUCount; i++)
                {
                    var pcPU = new PredictionUnit { ParentCU = pcLCU };
                    GetPUOffsetAndSize(pcLCU.Size, (PartitionSize)CUMode, i, out var PUOffsetX, out var PUOffsetY, out var PUWidth, out var PUHeight);
                    int PUX = pcLCU.PixelX + PUOffsetX;
                    int PUY = pcLCU.PixelY + PUOffsetY;
                    pcPU.X = PUX;
                    pcPU.Y = PUY;
                    pcPU.Width = PUWidth;
                    pcPU.Height = PUHeight;
                    pcLCU.PUs.Add(pcPU);
                }
            }
            return true;
        }

        public static void WriteBitmaps(CodingUnit cu, WriteableBitmap writeableBitmap)
        {
            foreach (var subCu in cu.SubCUs)
            {
                WriteBitmaps(subCu, writeableBitmap);
            }

            using (writeableBitmap.GetBitmapContext())
            {
                foreach (var pu in cu.PUs)
                {
                    writeableBitmap.DrawRectangle(pu.X, pu.Y, pu.X + pu.Width, pu.Y + pu.Height, Colors.Yellow);
                }

                writeableBitmap.DrawRectangle(cu.PixelX, cu.PixelY, cu.PixelX + cu.Size, cu.PixelY + cu.Size, Colors.Blue);
            }
        }

        private void GetPUOffsetAndSize(int leafCUSize,
                                        PartitionSize partitionSize,
                                        int PUIdx,
                                        out int xOffset,
                                        out int yOffset,
                                        out int width,
                                        out int height)
        {
            switch (partitionSize)
            {
                case PartitionSize.SIZE_2NxN:
                    width = leafCUSize;
                    height = leafCUSize >> 1;
                    xOffset = 0;
                    yOffset = (PUIdx == 0) ? 0 : leafCUSize >> 1;
                    break;
                case PartitionSize.SIZE_Nx2N:
                    width = leafCUSize >> 1;
                    height = leafCUSize;
                    xOffset = (PUIdx == 0) ? 0 : leafCUSize >> 1;
                    yOffset = 0;
                    break;
                case PartitionSize.SIZE_NxN:
                    width = leafCUSize >> 1;
                    height = leafCUSize >> 1;
                    xOffset = ((PUIdx & 1) == 0) ? 0 : leafCUSize >> 1;
                    yOffset = ((PUIdx & 2) == 0) ? 0 : leafCUSize >> 1;
                    break;
                case PartitionSize.SIZE_2NxnU:
                    width = leafCUSize;
                    height = (PUIdx == 0) ? leafCUSize >> 2 : (leafCUSize >> 2) + (leafCUSize >> 1);
                    xOffset = 0;
                    yOffset = (PUIdx == 0) ? 0 : leafCUSize >> 2;
                    break;
                case PartitionSize.SIZE_2NxnD:
                    width = leafCUSize;
                    height = (PUIdx == 0) ? (leafCUSize >> 2) + (leafCUSize >> 1) : leafCUSize >> 2;
                    xOffset = 0;
                    yOffset = (PUIdx == 0) ? 0 : (leafCUSize >> 2) + (leafCUSize >> 1);
                    break;
                case PartitionSize.SIZE_nLx2N:
                    width = (PUIdx == 0) ? leafCUSize >> 2 : (leafCUSize >> 2) + (leafCUSize >> 1);
                    height = leafCUSize;
                    xOffset = (PUIdx == 0) ? 0 : leafCUSize >> 2;
                    yOffset = 0;
                    break;
                case PartitionSize.SIZE_nRx2N:
                    width = (PUIdx == 0) ? (leafCUSize >> 2) + (leafCUSize >> 1) : leafCUSize >> 2;
                    height = leafCUSize;
                    xOffset = (PUIdx == 0) ? 0 : (leafCUSize >> 2) + (leafCUSize >> 1);
                    yOffset = 0;
                    break;
                default:
                    width = leafCUSize;
                    height = leafCUSize;
                    xOffset = 0;
                    yOffset = 0;
                    break;
            }
        }

        private int GetPUNumber(PartitionSize partitionSize)
        {
            int totalNumber;
            switch (partitionSize)
            {
                case PartitionSize.SIZE_2Nx2N:
                    totalNumber = 1;
                    break;
                case PartitionSize.SIZE_NxN:
                    totalNumber = 4;
                    break;
                case PartitionSize.SIZE_2NxN:
                case PartitionSize.SIZE_Nx2N:
                case PartitionSize.SIZE_2NxnU:
                case PartitionSize.SIZE_2NxnD:
                case PartitionSize.SIZE_nLx2N:
                case PartitionSize.SIZE_nRx2N:
                    totalNumber = 2;
                    break;
                default:
                    totalNumber = 0;
                    break;
            }

            return totalNumber;
        }
    }
}
