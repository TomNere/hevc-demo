using System.Collections.Generic;

namespace HEVCDemo.Hevc
{
    public class ComFrame
    {
        public int Poc;
        public VideoSequence Sequence;
        public List<CodingUnit> CodingUnits = new List<CodingUnit>();

        public CodingUnit GetCUByAddress(int address)
        {
            foreach(var cu in CodingUnits)
            {
                if (cu.Address == address) return cu;
            }

            return null;
        }
    }
}
