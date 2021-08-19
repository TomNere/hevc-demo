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
    public class IntraParser
    {
        private readonly VideoSequence videoSequence;

        public IntraParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(CacheProvider cacheProvider)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.IntraFilePath);
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
                            XReadIntraMode(tokens, pcLCU, ref index);

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
                    MessageBox.Show($"{"ErrorParsingIntraEx,Text".Localize()}\n\n{e.Message}");
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
                if (pu.PredictionMode != PredictionMode.MODE_INTRA) continue;

                using (writeableBitmap.GetBitmapContext())
                {
                    switch (pu.IntraDirLuma)
                    {
                        case 0: // Planar
                            writeableBitmap.DrawEllipse(pu.X, pu.Y, pu.X + pu.Width, pu.Y + pu.Height, Colors.Yellow);
                            break;
                        case 1: // DC
                            writeableBitmap.DrawLine(pu.X, pu.Y + pu.Height / 2, pu.X + pu.Width / 2, pu.Y, Colors.Yellow);
                            break;
                        default:
                            if (pu.IntraDirLuma >= 2 && pu.IntraDirLuma <= 17)
                            {
                                var offset = pu.IntraDirLuma - 1; // 2-17 => 1-16
                                var scaled = (pu.Height / 16) * offset;
                                writeableBitmap.DrawLine(pu.X, pu.Y + (pu.Height - scaled), pu.X + (pu.Width / 2), pu.Y + (pu.Height / 2), Colors.Red);
                            }
                            else if (pu.IntraDirLuma >= 18 && pu.IntraDirLuma <= 34)
                            {
                                var offset = pu.IntraDirLuma - 18;
                                var scaled = (pu.Width / 16) * offset;
                                writeableBitmap.DrawLine(pu.X + scaled, pu.Y, pu.X + (pu.Width / 2), pu.Y + (pu.Height / 2), Colors.Blue);
                            }
                            break;
                    }
                }
            }
        }

        public bool XReadIntraMode(string[] tokens, ComCU pcLCU, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            if (pcLCU.SCUs.Count > 0)
            {
                /// non-leaf node : recursive reading for children
                XReadIntraMode(tokens, pcLCU.SCUs[0], ref index);
                XReadIntraMode(tokens, pcLCU.SCUs[1], ref index);
                XReadIntraMode(tokens, pcLCU.SCUs[2], ref index);
                XReadIntraMode(tokens, pcLCU.SCUs[3], ref index);
            }
            else
            {
                /// leaf node : read data
                foreach(var pcPU in pcLCU.PUs)
                {
                    pcPU.IntraDirLuma = int.Parse(tokens[index++]);
                    pcPU.IntraDirChroma = int.Parse(tokens[index++]);
                }
            }
            return true;
        }
    }
}
