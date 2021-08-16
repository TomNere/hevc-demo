using System.Collections.Generic;

namespace HEVCDemo.Types
{
    public class ComFrame
    {
        public int POC;
        public VideoSequence Sequence;
        public List<ComCU> CodingUnits = new List<ComCU>();

        public ComCU GetCUByAddress(int address)
        {
            foreach(var cu in CodingUnits)
            {
                if (cu.iAddr == address) return cu;
            }

            return null;
        }
    }
}
