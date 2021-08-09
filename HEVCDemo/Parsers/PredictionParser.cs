using HEVCDemo.Helpers;
using HEVCDemo.Types;
using Rasyidf.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HEVCDemo.Parsers
{
    public class PredictionParser
    {
        private readonly VideoSequence videoSequence;

        public PredictionParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(CacheProvider cacheProvider)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.PredictionFilePath);
                    string strOneLine = file.ReadLine();
                    int iDecOrder = -1;
                    int iLastPOC = -1;
                    /// <1,1> 99 0 0 5 0
                    while (strOneLine != null)
                    {
                        if (strOneLine[0] != '<')
                        {
                            // Line must start with <
                            throw new FormatException("InvalidPredictionFormatEx,Text".Localize());
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
                            XReadPredictionMode(tokens, pcLCU, ref index);

                            strOneLine = file.ReadLine();
                            if (strOneLine == null || int.Parse(strOneLine.Substring(1, strOneLine.LastIndexOf(',') - 1)) != frameNumber)
                            {
                                //WriteBitmaps(cacheProvider, cuRectangles, puRectangles, frameNumber);
                                break;
                            }
                        }
                    }
                    file.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{"ErrorParsingPredictionEx,Text".Localize()}\n\n{e.Message}");
                    return false;
                }

                return true;
            });
        }

        public bool XReadPredictionMode(string[] tokens, ComCU pcLCU, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            if (pcLCU.SCUs.Count > 0)
            {
                /// non-leaf node : recursive reading for children
                XReadPredictionMode(tokens, pcLCU.SCUs[0], ref index);
                XReadPredictionMode(tokens, pcLCU.SCUs[1], ref index);
                XReadPredictionMode(tokens, pcLCU.SCUs[2], ref index);
                XReadPredictionMode(tokens, pcLCU.SCUs[3], ref index);
            }
            else
            {
                /// leaf node : read data
                int iPredMode;
                foreach(var pcPU in pcLCU.PUs)
                {
                    iPredMode = int.Parse(tokens[index++]);
                    pcPU.PredictionMode = (PredictionMode)iPredMode;
                }
            }
            return true;
        }
    }
}
