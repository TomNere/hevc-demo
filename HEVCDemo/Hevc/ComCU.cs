using System.Collections.Generic;

namespace HEVCDemo.Hevc
{
    public class ComCU
    {
        public int iAddr;
        public int iPixelX;
        public int iPixelY;
        public int Size;
        public int Depth;
        public int Zorder;
        public ComFrame Frame;
        public List<ComCU> SCUs = new List<ComCU>();
        public List<ComPU> PUs = new List<ComPU>();
        public PartSize PartSize;
    }
}
