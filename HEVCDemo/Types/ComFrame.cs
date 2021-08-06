using System.Collections.Generic;

namespace HEVCDemo.Types
{
    public class ComFrame
    {
        public int POC;
        public VideoSequence Sequence;
        public List<ComCU> CodingUnits = new List<ComCU>();
    }
}
