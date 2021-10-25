using System.Collections.Generic;
using System.Linq;

namespace HEVCDemo.Types
{
    public class VideoSequence
    {
        public int Width;
        public int Height;
        public int MaxCUSize;
        public int MaxCUDepth;

        public int FramesCount => FramesInDecodeOrder.Count;

        public Dictionary<int, ComFrame> FramesInDecodeOrder = new Dictionary<int, ComFrame>();

        public ComFrame GetFrameByPoc(int poc)
        {
            return FramesInDecodeOrder.Values.FirstOrDefault(frame => frame.POC == poc);
        }
    }
}
