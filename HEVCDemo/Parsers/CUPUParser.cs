using HEVCDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Parsers
{
    public class CUPUParser
    {
        private int iSeqWidth = 1920;
        private int iMaxCUSize = 64;

        public bool ParseFile(CacheProvider cacheProvider)
        {
            int iCUOneRow = (iSeqWidth + iMaxCUSize - 1) / iMaxCUSize;

            /// <1,1> 99 0 0 5 0
            /// read one LCU
            //ComFrame* pcFrame = NULL;
            //ComCU* pcLCU = NULL;
            int iLCUSize = iMaxCUSize; //pcSequence->getMaxCUSize();
            //cMatchTarget.setPattern("^<(-?[0-9]+),([0-9]+)> (.*) ");
            //QTextStream cCUInfoStream;
            int iDecOrder = -1;
            int iLastPOC = -1;

            //var bitmaps = new Dictionary<int, BitmapSource>();

            try
            {
                var file = new System.IO.StreamReader(cacheProvider.CupuFilePath);
                string strOneLine = file.ReadLine();

                while (strOneLine != null)
                {
                    if (strOneLine[0] != '<')
                    {
                        // Line must start with <
                        throw new FormatException();
                    }

                    int frameNumber = int.Parse(strOneLine.Substring(1, strOneLine.LastIndexOf(',') - 1));
                    var cuRectangles = new List<Rectangle>();
                    var puRectangles = new List<Rectangle>();

                    while (true)
                    {
                        int addressStart = strOneLine.LastIndexOf(',');
                        int addressEnd = strOneLine.LastIndexOf('>');
                        int iAddr = int.Parse(strOneLine.Substring(addressStart + 1, addressEnd - addressStart - 1));
                        var tokens = strOneLine.Substring(addressEnd + 2).Split(' ');

                        /// poc and lcu addr
                        //int iPoc = cMatchTarget.cap(1).toInt();
                        //iDecOrder += (iLastPOC != iPoc);
                        //iLastPOC = iPoc;
                        //pcFrame = pcSequence->getFramesInDecOrder().at(iDecOrder);
                        var pcLCU = new ComCU { iAddr = iAddr };
                        //pcLCU->setAddr(iAddr);
                        //pcLCU->setFrame(pcFrame);
                        //pcLCU->setDepth(0);
                        //pcLCU->setZorder(0);
                        //pcLCU->setSize(iLCUSize);
                        pcLCU.iPixelX = (iAddr % iCUOneRow) * iMaxCUSize;
                        pcLCU.iPixelY = (iAddr / iCUOneRow) * iMaxCUSize;
                        pcLCU.Size = iLCUSize;
                        //pcLCU->setX(iPixelX);
                        //pcLCU->setY(iPixelY);

                        /// recursively parse the CU&PU quard-tree structure
                        //QString strCUInfo = cMatchTarget.cap(3);
                        //cCUInfoStream.setString(&strCUInfo, QIODevice::ReadOnly);
                        puRectangles.Add(new Rectangle { X = pcLCU.iPixelX, Y = pcLCU.iPixelY, Height = pcLCU.Size, Width = pcLCU.Size });
                        var index = 0;
                        if (!xReadInCUMode(tokens, pcLCU, cuRectangles, puRectangles, ref index))
                        {
                            throw new FormatException();
                        }
                        else
                        {
                            
                        }

                        strOneLine = file.ReadLine();
                        if (strOneLine == null || int.Parse(strOneLine.Substring(1, strOneLine.LastIndexOf(',') - 1)) != frameNumber)
                        {
                            var writeableBitmap = BitmapFactory.New(1920, 1080);
                            //Graphics g = Graphics.FromImage(bitmap);

                            //Image img = Bitmap.FromFile(LoadPath);
                            //Image img2 = Bitmap.FromFile(TempPath);

                            //g.DrawImage(img, 0, 0);
                            //g.DrawImage(img2, 250, 250);

                            //var pen = new System.Drawing.Pen(System.Drawing.Brushes.Black, 1);

                            using (writeableBitmap.GetBitmapContext())
                            {
                                for (int i = 0; i < cuRectangles.Count; i++)
                                {
                                    var rect = cuRectangles[i];
                                    //g.DrawRectangle(pen, rect);
                                    writeableBitmap.DrawRectangle(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, Colors.Blue);
                                }
                                for (int i = 0; i < puRectangles.Count; i++)
                                {
                                    var rect = puRectangles[i];
                                    writeableBitmap.DrawRectangle(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, Colors.Yellow);
                                }
                            }

                            cacheProvider.SaveBitmap(writeableBitmap, frameNumber, CacheProvider.CacheItemType.Cupu);
                            //bitmaps.Add(frameNumber, writeableBitmap);
                            break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show($"Error parsing CUPU file!\n\n{e.Message}");
                return false;
            }

            return true;
        }

        public bool xReadInCUMode(string[] tokens, ComCU pcLCU, List<Rectangle> cuRectangles, List<Rectangle> puRectangles, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            var iCUMode = int.Parse(tokens[index]);

            if (iCUMode == 99)
            {
                int iMaxDepth = 4;// pcCU->getFrame()->getSequence()->getMaxCUDepth();
                //int iTotalNumPart =     1 << ((iMaxDepth - pcCU->getDepth()) << 1);
                /// non-leaf node : add 4 children CUs
                for (int i = 0; i < 4; i++)
                {
                    var pcChildNode = new ComCU { iAddr = pcLCU.iAddr, Size = pcLCU.Size / 2, };

                    //pcChildNode->setAddr(pcCU->getAddr());
                    //pcChildNode->setDepth(pcCU->getDepth() + 1);
                    //pcChildNode->setZorder(pcCU->getZorder() + (iTotalNumPart / 4) * i);
                    //pcChildNode->setSize(pcCU->getSize() / 2);
                    int iSubCUX = pcLCU.iPixelX + i % 2 * (pcLCU.Size / 2);
                    int iSubCUY = pcLCU.iPixelY + i / 2 * (pcLCU.Size / 2);
                    pcChildNode.iPixelX = iSubCUX;
                    pcChildNode.iPixelY = iSubCUY;
                    //pcCU->getSCUs().push_back(pcChildNode);
                    cuRectangles.Add(new Rectangle { X = iSubCUX, Y = iSubCUY, Width = pcLCU.Size / 2, Height = pcLCU.Size / 2 });
                    index++;
                    xReadInCUMode(tokens, pcChildNode, cuRectangles, puRectangles, ref index);//, pcChildNode);
                }
            }
            else
            {
                /// leaf node : create PUs and write the PU Mode for it
                //pcCU->setPartSize((PartSize)iCUMode);

                int iPUCount = getPUNum((PartSize)iCUMode);
                for (int i = 0; i < iPUCount; i++)
                {
                    //rectangles.Add(new Rectangle { X = pcLCU.iPixelX, Y = pcLCU.iPixelY, Width = pcLCU.Size, Height = pcLCU.Size});

                    //ComPU* pcPU = new ComPU(pcCU);
                    getPUOffsetAndSize(pcLCU.Size, (PartSize)iCUMode, i, out var iPUOffsetX, out var iPUOffsetY, out var iPUWidth, out var iPUHeight);
                    int iPUX = pcLCU.iPixelX + iPUOffsetX;
                    int iPUY = pcLCU.iPixelY + iPUOffsetY;
                    //pcPU->setX(iPUX);
                    //pcPU->setY(iPUY);
                    //pcPU->setWidth(iPUWidth);
                    //pcPU->setHeight(iPUHeight);
                    //pcCU->getPUs().push_back(pcPU);
                    //puRectangles.Add(new Rectangle { X = iPUX, Y = iPUY, Width = iPUWidth, Height = iPUHeight });
                }
            }
            return true;
        }

        void getPUOffsetAndSize(int iLeafCUSize,
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
                    //Q_ASSERT(ePartSize == SIZE_2Nx2N);
                    riWidth = iLeafCUSize;
                    riHeight = iLeafCUSize;
                    riXOffset = 0;
                    riYOffset = 0;
                    break;
            }
        }

        int getPUNum(PartSize ePartSize)
        {
            int iTotalNum = 0;
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
                    //Q_ASSERT(ePartSize == SIZE_NONE); ///< SIZE_NONE (out of boundary)
                    iTotalNum = 0;
                    break;
            }
            return iTotalNum;
        }

        enum PartSize
        {
            SIZE_2Nx2N,           ///< symmetric motion partition,  2Nx2N
            SIZE_2NxN,            ///< symmetric motion partition,  2Nx N
            SIZE_Nx2N,            ///< symmetric motion partition,   Nx2N
            SIZE_NxN,             ///< symmetric motion partition,   Nx N

            SIZE_2NxnU,           ///< asymmetric motion partition, 2Nx( N/2) + 2Nx(3N/2)
            SIZE_2NxnD,           ///< asymmetric motion partition, 2Nx(3N/2) + 2Nx( N/2)
            SIZE_nLx2N,           ///< asymmetric motion partition, ( N/2)x2N + (3N/2)x2N
            SIZE_nRx2N,           ///< asymmetric motion partition, (3N/2)x2N + ( N/2)x2N

            SIZE_NONE = 15
        };

        public class ComCU
        {
            public int iAddr;
            public int iPixelX;
            public int iPixelY;
            public int Size;
        }
    }
}
