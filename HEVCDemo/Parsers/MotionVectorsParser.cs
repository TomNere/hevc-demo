using HEVCDemo.Helpers;
using HEVCDemo.Types;
using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Parsers
{
    public class MotionVectorsParser
    {
        private readonly VideoSequence videoSequence;

        public MotionVectorsParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(CacheProvider cacheProvider)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.MotionVectorsFilePath);
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
                            iLastPOC = iPoc;
                            var tokens = strOneLine.Substring(addressEnd + 2).Split(' ');

                            var frame = videoSequence.FramesInDecodeOrder[iDecOrder];

                            var pcLCU = frame.GetCUByAddress(iAddr);

                            var index = 0;
                            XReadMotionVectors(tokens, pcLCU, ref index);

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
                    MessageBox.Show($"{"ErrorParsingVectorsEx,Text".Localize()}\n\n{e.Message}");
                    return false;
                }

                return true;
            });
        }

        public static void WriteBitmaps(ComCU cu, WriteableBitmap writeableBitmap)
        {
            foreach (var sCu in cu.SCUs)
            {
                WriteBitmaps(sCu, writeableBitmap);
            }

            foreach (var pu in cu.PUs)
            {
                //TODO   
            }
        }

        public bool XReadMotionVectors(string[] tokens, ComCU pcLCU, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            if (pcLCU.SCUs.Count > 0)
            {
                /// non-leaf node : recursive reading for children
                XReadMotionVectors(tokens, pcLCU.SCUs[0], ref index);
                XReadMotionVectors(tokens, pcLCU.SCUs[1], ref index);
                XReadMotionVectors(tokens, pcLCU.SCUs[2], ref index);
                XReadMotionVectors(tokens, pcLCU.SCUs[3], ref index);
            }
            else
            {
                /// leaf node : read data
                foreach(var pcPU in pcLCU.PUs)
                {
                    pcPU.InterDir = int.Parse(tokens[index++]);
                    int vectorsCount = 0;

                    if (pcPU.InterDir == 1 || pcPU.InterDir == 2)   //uni-prediction, 1 MV
                    {
                        vectorsCount = 1;
                    }
                    else if (pcPU.InterDir == 3)                    //bi-prediction, 2 MVs
                    {
                        vectorsCount = 2;
                    }

                    for (int i = 0; i < vectorsCount; i++)
                    {
                        var motionVector = new MotionVector
                        {
                            RefPoc = int.Parse(tokens[index++]),
                            Horizontal = int.Parse(tokens[index++]),
                            Vertical = int.Parse(tokens[index++])
                        };
                        pcPU.MotionVectors.Add(motionVector);
                    }
                }
            }
            return true;
        }
    }
}
