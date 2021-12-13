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
    public class IntraPredictionModeParser
    {
        private readonly VideoSequence videoSequence;

        public IntraPredictionModeParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(VideoCache cacheProvider)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.IntraPredictionFilePath);
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
                            lastPOC = poc;
                            var tokens = strOneLine.Substring(addressEnd + 2).Split(' ');

                            var frame = videoSequence.FramesInDecodeOrder[decOrder];

                            var pcLCU = frame.GetCUByAddress(address);

                            var index = 0;
                            ReadIntraPredictionMode(tokens, pcLCU, ref index);

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

        public static void WriteBitmaps(CodingUnit cu, WriteableBitmap writeableBitmap)
        {
            foreach (var sCu in cu.SubCUs)
            {
                WriteBitmaps(sCu, writeableBitmap);
            }

            foreach (var pu in cu.PUs)
            {
                if (pu.PredictionType != PredictionType.INTRA) continue;

                using (writeableBitmap.GetBitmapContext())
                {
                    switch (pu.IntraDirLuma)
                    {
                        case 0: // Planar
                            writeableBitmap.DrawEllipse(pu.X, pu.Y, pu.X + pu.Width, pu.Y + pu.Height, Colors.Green);
                            break;
                        case 1: // DC
                            writeableBitmap.DrawLine(pu.X, pu.Y + pu.Height / 2, pu.X + pu.Width / 2, pu.Y, Colors.Green);
                            break;
                        default: // Angulars
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

        public bool ReadIntraPredictionMode(string[] tokens, CodingUnit pcLCU, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            if (pcLCU.SubCUs.Count > 0)
            {
                /// Non-leaf node - recursive reading for children
                ReadIntraPredictionMode(tokens, pcLCU.SubCUs[0], ref index);
                ReadIntraPredictionMode(tokens, pcLCU.SubCUs[1], ref index);
                ReadIntraPredictionMode(tokens, pcLCU.SubCUs[2], ref index);
                ReadIntraPredictionMode(tokens, pcLCU.SubCUs[3], ref index);
            }
            else
            {
                /// Leaf node - read data
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
