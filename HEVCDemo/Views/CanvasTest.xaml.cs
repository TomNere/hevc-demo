using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for CanvasTest
    /// </summary>
    public partial class CanvasTest : UserControl
    {
        public CanvasTest()
        {
            InitializeComponent();
        }

        private int iSeqWidth = 1920;
        private int iMaxCUSize = 64;

        private bool ReadFile()
        {
            int iCUOneRow = (iSeqWidth + iMaxCUSize - 1) / iMaxCUSize;

            ////
            string strOneLine;

            /// <1,1> 99 0 0 5 0
            /// read one LCU
            //ComFrame* pcFrame = NULL;
            //ComCU* pcLCU = NULL;
            int iLCUSize = iMaxCUSize; //pcSequence->getMaxCUSize();
            //cMatchTarget.setPattern("^<(-?[0-9]+),([0-9]+)> (.*) ");
            //QTextStream cCUInfoStream;
            int iDecOrder = -1;
            int iLastPOC = -1;

            var file = new System.IO.StreamReader(@"./test.txt");

            this.RectanglesCanvas.Rectangles.Clear();
            for (int i = 0; i < 509; i++)
            {
                strOneLine = file.ReadLine();
                var start = strOneLine.LastIndexOf(',');
                var end = strOneLine.LastIndexOf('>');
                var iAddr = int.Parse(strOneLine.Substring(start + 1, end - start - 1));
                var tokens = strOneLine.Substring(end + 2).Split(' ');

                if (true)
                {
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
                    if (xReadInCUMode(tokens, pcLCU) == false)
                        return false;
                    //pcFrame->getLCUs().push_back(pcLCU);
                }

                /// sort LCU in ascendning order
                //qSort(pcFrame->getLCUs().begin(), pcFrame->getLCUs().end(), xCUSortingOrder);

            }
            this.Dispatcher.Invoke(() => this.RectanglesCanvas.InvalidateVisual());
            return true;
        }

        public bool xReadInCUMode(string[] tokens, ComCU pcLCU, int index = 0)
        {
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
                    xReadInCUMode(tokens, pcChildNode, index + 1);//, pcChildNode);
                }
            }
            else
            {
                /// leaf node : create PUs and write the PU Mode for it
                //pcCU->setPartSize((PartSize)iCUMode);

                int iPUCount = getPUNum((PartSize)iCUMode);
                for (int i = 0; i < iPUCount; i++)
                {
                    //ComPU* pcPU = new ComPU(pcCU);
                    getPUOffsetAndSize(pcLCU.Size, (PartSize)iCUMode, i, out var iPUOffsetX, out var iPUOffsetY, out var iPUWidth, out var iPUHeight);
                    int iPUX = pcLCU.iPixelX + iPUOffsetX;
                    int iPUY = pcLCU.iPixelY + iPUOffsetY;
                    //pcPU->setX(iPUX);
                    //pcPU->setY(iPUY);
                    //pcPU->setWidth(iPUWidth);
                    //pcPU->setHeight(iPUHeight);
                    //pcCU->getPUs().push_back(pcPU);
                    this.RectanglesCanvas.Rectangles.Add(new Rectangle { X = iPUX, Y = iPUY, Width = iPUWidth, Height = iPUHeight });
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.RectanglesCanvas.Rectangles.Add(new Rect(100, 100, 100, 100));
            //this.RectanglesCanvas.InvalidateVisual();
            this.ReadFile();

            

        }
    }
}
