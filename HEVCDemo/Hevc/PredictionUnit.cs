using System.Collections.Generic;

namespace HEVCDemo.Hevc
{
    public class PredictionUnit
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public PredictionType PredictionType;
        public CodingUnit ParentCU;
        public int IntraDirLuma = -1;
        public int IntraDirChroma = -1;  // Not shown anywhere
        public int InterDir;
        public List<MotionVector> MotionVectors = new List<MotionVector>();
    }
}
