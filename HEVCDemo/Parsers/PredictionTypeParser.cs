﻿using HEVCDemo.Hevc;
using HEVCDemo.Models;
using Rasyidf.Localization;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Parsers
{
    public class PredictionTypeParser
    {
        private static readonly Color intraColor = Color.FromArgb(100, 255, 66, 151);
        private static readonly Color interColor = Color.FromArgb(100, 0, 89, 255);

        private readonly VideoSequence videoSequence;

        public PredictionTypeParser(VideoSequence videoSequence)
        {
            this.videoSequence = videoSequence;
        }

        public async Task<bool> ParseFile(VideoCache cacheProvider)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new System.IO.StreamReader(cacheProvider.PredictionTypeFilePath);
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
                            ReadPredictionType(tokens, pcLCU, ref index);

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
                using (writeableBitmap.GetBitmapContext())
                {
                    var rect = new System.Drawing.Rectangle(pu.X, pu.Y, pu.Width, pu.Height);
                    var color = Colors.Transparent;

                    switch (pu.PredictionType)
                    {
                        case PredictionType.SKIP:
                            continue;
                        case PredictionType.INTER:
                            color = interColor;
                            break;
                        case PredictionType.INTRA:
                            color = intraColor;
                            break;
                        case PredictionType.NONE:
                            continue;
                    }

                    writeableBitmap.FillRectangle(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, color);
                }
            }
        }

        public bool ReadPredictionType(string[] tokens, CodingUnit pcLCU, ref int index)
        {
            if (index > tokens.Length - 1)
            {
                return false;
            }

            if (pcLCU.SubCUs.Count > 0)
            {
                /// Non-leaf node - recursive reading for children
                ReadPredictionType(tokens, pcLCU.SubCUs[0], ref index);
                ReadPredictionType(tokens, pcLCU.SubCUs[1], ref index);
                ReadPredictionType(tokens, pcLCU.SubCUs[2], ref index);
                ReadPredictionType(tokens, pcLCU.SubCUs[3], ref index);
            }
            else
            {
                /// Leaf node - read data
                int iPredType;
                foreach(var pcPU in pcLCU.PUs)
                {
                    iPredType = int.Parse(tokens[index++]);
                    pcPU.PredictionType = (PredictionType)iPredType;
                }
            }

            return true;
        }
    }
}
