using System.Collections.Generic;

namespace HEVCDemo.Hevc
{
    public class CodingUnit
    {
        public int Address;
        public int PixelX;
        public int PixelY;
        public int Size;
        public int Depth;
        public int ZOrder;
        public ComFrame Frame;
        public List<CodingUnit> SubCUs = new List<CodingUnit>();
        public List<PredictionUnit> PUs = new List<PredictionUnit>();
        public PartitionSize PartitionSize;
    }
}
