using HEVCDemo.Helpers;
using HEVCDemo.Types;
using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Parsers
{
    public class PredictionParser
    {
        private static readonly Color intraColor = Color.FromArgb(100, 255, 66, 151);
        private static readonly Color interColor = Color.FromArgb(100, 0, 89, 255);

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
                    var file = new System.IO.StreamReader(cacheProvider.PredictionTypeFilePath);
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
                            XReadPredictionMode(tokens, pcLCU, ref index);

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
                    MessageBox.Show($"{"ErrorParsingPredictionEx,Text".Localize()}\n\n{e.Message}");
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
                using (writeableBitmap.GetBitmapContext())
                {
                    var rect = new System.Drawing.Rectangle(pu.X, pu.Y, pu.Width, pu.Height);
                    var color = Colors.Transparent;

                    switch (pu.PredictionMode)
                    {
                        case PredictionMode.MODE_SKIP:
                            continue;
                        case PredictionMode.MODE_INTER:
                            color = interColor;
                            break;
                        case PredictionMode.MODE_INTRA:
                            color = intraColor;
                            break;
                        case PredictionMode.MODE_NONE:
                            continue;
                    }

                    writeableBitmap.FillRectangle(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, color);
                }
            }
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
