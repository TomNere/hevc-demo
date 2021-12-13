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

                int iCUOneRow = (videoSequence.Width + videoSequence.MaxCUSize - 1) / videoSequence.MaxCUSize;
                int iLCUSize = videoSequence.MaxCUSize;

                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.CodingPredictionUnitsFilePath);
                    string strOneLine = file.ReadLine();
                    int iDecOrder = -1;
                    int iLastPOC = -1;

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
                            int iPoc = int.Parse(strOneLine.Substring(pocStart + 1, addressStart - pocStart - 1));
                            int iAddr = int.Parse(strOneLine.Substring(addressStart + 1, addressEnd - addressStart - 1));

                            iDecOrder += iLastPOC != iPoc ? 1 : 0;
                            ComFrame frame;

                            if (iPoc != iLastPOC)
                            {
                                frame = new ComFrame { Poc = iPoc, Sequence = videoSequence };
                                videoSequence.FramesInDecodeOrder.Add(iDecOrder, frame);
                            }
                            else
                            {
                                frame = videoSequence.FramesInDecodeOrder[iDecOrder];
                            }

                            iLastPOC = iPoc;

                            var tokens = strOneLine.Substring(addressEnd + 2).Split(' ');

                            /// Poc and Lcu addr
                            var pcLCU = new ComCU { iAddr = iAddr, Frame = frame, Size = iLCUSize };
                            pcLCU.iPixelX = (iAddr % iCUOneRow) * videoSequence.MaxCUSize;
                            pcLCU.iPixelY = (iAddr / iCUOneRow) * videoSequence.MaxCUSize;

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

        public bool ReadInCUMode(string[] tokens, ComCU pcLCU, ref int index)
        {
            if (index > tokens.Length - 1) return false;

            var iCUMode = int.Parse(tokens[index]);

            if (iCUMode == 99)
            {
                int iMaxDepth = pcLCU.Frame.Sequence.MaxCUDepth;
                int iTotalNumPart = 1 << ((iMaxDepth - pcLCU.Depth) << 1);

                /// Non-leaf node, add 4 children CUs
                for (int i = 0; i < 4; i++)
                {
                    var pcChildNode = new ComCU 
                    {
                        Frame = pcLCU.Frame,
                        iAddr = pcLCU.iAddr, 
                        Size = pcLCU.Size / 2,
                        Depth = pcLCU.Depth + 1,
                        Zorder = pcLCU.Zorder + (iTotalNumPart/4) * i
                    };

                    int iSubCUX = pcLCU.iPixelX + i % 2 * (pcLCU.Size / 2);
                    int iSubCUY = pcLCU.iPixelY + i / 2 * (pcLCU.Size / 2);
                    pcChildNode.iPixelX = iSubCUX;
                    pcChildNode.iPixelY = iSubCUY;
                    pcLCU.SCUs.Add(pcChildNode);
                    index++;
                    ReadInCUMode(tokens, pcChildNode, ref index);
                }
            }
            else
            {
                /// Leaf node - create PUs and write the PU Mode for it
                pcLCU.PartSize = (PartSize)iCUMode;

                int iPUCount = GetPUNumber((PartSize)iCUMode);
                for (int i = 0; i < iPUCount; i++)
                {
                    var pcPU = new ComPU { CU = pcLCU };
                    GetPUOffsetAndSize(pcLCU.Size, (PartSize)iCUMode, i, out var iPUOffsetX, out var iPUOffsetY, out var iPUWidth, out var iPUHeight);
                    int iPUX = pcLCU.iPixelX + iPUOffsetX;
                    int iPUY = pcLCU.iPixelY + iPUOffsetY;
                    pcPU.X = iPUX;
                    pcPU.Y = iPUY;
                    pcPU.Width = iPUWidth;
                    pcPU.Height = iPUHeight;
                    pcLCU.PUs.Add(pcPU);
                }
            }
            return true;
        }

        public static void WriteBitmaps(ComCU cu, WriteableBitmap writeableBitmap)
        {
            foreach (var sCu in cu.SCUs)
            {
                WriteBitmaps(sCu, writeableBitmap);
            }

            using (writeableBitmap.GetBitmapContext())
            {
                foreach (var pu in cu.PUs)
                {
                    writeableBitmap.DrawRectangle(pu.X, pu.Y, pu.X + pu.Width, pu.Y + pu.Height, Colors.Yellow);
                }

                writeableBitmap.DrawRectangle(cu.iPixelX, cu.iPixelY, cu.iPixelX + cu.Size, cu.iPixelY + cu.Size, Colors.Blue);
            }
        }

        private void GetPUOffsetAndSize(int iLeafCUSize,
                                        PartSize ePartSize,
                                        int uiPUIdx,
                                        out int riXOffset,
                                        out int riYOffset,
                                        out int riWidth,
                                        out int riHeight)
        {
            switch (ePartSize)
            {
                case PartSize.SIZE_2NxN:
                    riWidth = iLeafCUSize;
                    riHeight = iLeafCUSize >> 1;
                    riXOffset = 0;
                    riYOffset = (uiPUIdx == 0) ? 0 : iLeafCUSize >> 1;
                    break;
                case PartSize.SIZE_Nx2N:
                    riWidth = iLeafCUSize >> 1;
                    riHeight = iLeafCUSize;
                    riXOffset = (uiPUIdx == 0) ? 0 : iLeafCUSize >> 1;
                    riYOffset = 0;
                    break;
                case PartSize.SIZE_NxN:
                    riWidth = iLeafCUSize >> 1;
                    riHeight = iLeafCUSize >> 1;
                    riXOffset = ((uiPUIdx & 1) == 0) ? 0 : iLeafCUSize >> 1;
                    riYOffset = ((uiPUIdx & 2) == 0) ? 0 : iLeafCUSize >> 1;
                    break;
                case PartSize.SIZE_2NxnU:
                    riWidth = iLeafCUSize;
                    riHeight = (uiPUIdx == 0) ? iLeafCUSize >> 2 : (iLeafCUSize >> 2) + (iLeafCUSize >> 1);
                    riXOffset = 0;
                    riYOffset = (uiPUIdx == 0) ? 0 : iLeafCUSize >> 2;
                    break;
                case PartSize.SIZE_2NxnD:
                    riWidth = iLeafCUSize;
                    riHeight = (uiPUIdx == 0) ? (iLeafCUSize >> 2) + (iLeafCUSize >> 1) : iLeafCUSize >> 2;
                    riXOffset = 0;
                    riYOffset = (uiPUIdx == 0) ? 0 : (iLeafCUSize >> 2) + (iLeafCUSize >> 1);
                    break;
                case PartSize.SIZE_nLx2N:
                    riWidth = (uiPUIdx == 0) ? iLeafCUSize >> 2 : (iLeafCUSize >> 2) + (iLeafCUSize >> 1);
                    riHeight = iLeafCUSize;
                    riXOffset = (uiPUIdx == 0) ? 0 : iLeafCUSize >> 2;
                    riYOffset = 0;
                    break;
                case PartSize.SIZE_nRx2N:
                    riWidth = (uiPUIdx == 0) ? (iLeafCUSize >> 2) + (iLeafCUSize >> 1) : iLeafCUSize >> 2;
                    riHeight = iLeafCUSize;
                    riXOffset = (uiPUIdx == 0) ? 0 : (iLeafCUSize >> 2) + (iLeafCUSize >> 1);
                    riYOffset = 0;
                    break;
                default:
                    riWidth = iLeafCUSize;
                    riHeight = iLeafCUSize;
                    riXOffset = 0;
                    riYOffset = 0;
                    break;
            }
        }

        private int GetPUNumber(PartSize ePartSize)
        {
            int iTotalNum;
            switch (ePartSize)
            {
                case PartSize.SIZE_2Nx2N:
                    iTotalNum = 1;
                    break;
                case PartSize.SIZE_NxN:
                    iTotalNum = 4;
                    break;
                case PartSize.SIZE_2NxN:
                case PartSize.SIZE_Nx2N:
                case PartSize.SIZE_2NxnU:
                case PartSize.SIZE_2NxnD:
                case PartSize.SIZE_nLx2N:
                case PartSize.SIZE_nRx2N:
                    iTotalNum = 2;
                    break;
                default:
                    iTotalNum = 0;
                    break;
            }

            return iTotalNum;
        }
    }
}
