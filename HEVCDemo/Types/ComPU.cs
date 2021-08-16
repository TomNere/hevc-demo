using System.Collections.Generic;

namespace HEVCDemo.Types
{
    public class ComPU
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public PredictionMode PredictionMode;
        public ComCU CU;
        public int IntraDirLuma = -1;
        public int IntraDirChroma = -1;
    }
}
