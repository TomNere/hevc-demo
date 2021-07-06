using HEVCDemo.Helpers;

namespace HEVCDemo.Parsers
{
    public class PropsParser
    {
        private const string maxCUHeightPrefix = "maxCUHeight";
        private const string seqWidthInLumaPrefix = "seqWidthInLuma";
        private const string seqHeightInLumaPrefix = "seqHeightInLuma";

        public int MaxCUHeight;
        public int SeqHeight;
        public int SeqWidth;

        public void ParseProps(CacheProvider cacheProvider)
        {
            var propsFile = new System.IO.StreamReader(cacheProvider.PropsFilePath);
            string line;
            while ((line = propsFile.ReadLine()) != null)
            {
                if (line.Contains(seqWidthInLumaPrefix))
                {
                    SeqWidth = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
                else if (line.Contains(seqHeightInLumaPrefix))
                {
                    SeqHeight = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
                else if (line.Contains(maxCUHeightPrefix))
                {
                    MaxCUHeight = int.Parse(line.Substring(line.IndexOf(":") + 1));
                }
            }
            propsFile.Close();
        }
    }
}
