using System.Collections.Generic;

namespace HEVCDemo.Types
{
    public class VideoSequence
    {
        public int Width;
        public int Height;
        public int FramesCount;
        public int MaxCUSize;
        public int MaxCUDepth;

        public Dictionary<int, ComFrame> FramesInDecodeOrder = new Dictionary<int, ComFrame>();
    }
}
