using HEVCDemo.Helpers;
using HEVCDemo.Types;

namespace HEVCDemo.Parsers
{
    public class PropsParser
    {
        private const string maxCUHeightPrefix = "maxCUHeight";
        private const string seqWidthInLumaPrefix = "seqWidthInLuma";
        private const string seqHeightInLumaPrefix = "seqHeightInLuma";

        public void ParseProps(CacheProvider cacheProvider, VideoSequence sequence)
        {
            var propsFile = new System.IO.StreamReader(cacheProvider.PropsFilePath);
            string line;
            while ((line = propsFile.ReadLine()) != null)
            {
                if (line.Contains(seqWidthInLumaPrefix))
                {
                    sequence.Width = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
                else if (line.Contains(seqHeightInLumaPrefix))
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
