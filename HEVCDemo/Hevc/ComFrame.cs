﻿using System.Collections.Generic;

namespace HEVCDemo.Hevc
{
    public class ComFrame
    {
        public int Poc;
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
