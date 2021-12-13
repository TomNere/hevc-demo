using HEVCDemo.Hevc;
using HEVCDemo.Models;

namespace HEVCDemo.Parsers
{
    public class SequencePropertiesParser
    {
        private const string maxCUHeightPrefix = "maxCUHeight";
        private const string sequenceWidthInLumaPrefix = "seqWidthInLuma";
        private const string sequenceHeightInLumaPrefix = "seqHeightInLuma";

        public void ParseSequenceProperties(VideoCache cacheProvider, VideoSequence sequence)
        {
            var propsFile = new System.IO.StreamReader(cacheProvider.SequencePropertiesFilePath);
            string line;

            while ((line = propsFile.ReadLine()) != null)
            {
                if (line.Contains(sequenceWidthInLumaPrefix))
                {
                    sequence.Width = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
                else if (line.Contains(sequenceHeightInLumaPrefix))
                {
                    sequence.Height = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
                else if (line.Contains(maxCUHeightPrefix))
                {
                    sequence.MaxCUSize = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
            }

            propsFile.Close();
        }
    }
}
