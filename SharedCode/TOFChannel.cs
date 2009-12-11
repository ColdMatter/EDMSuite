using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    public class TOFChannel : Channel<TOFWithError>
    {
        public TOFWithError Difference
        {
            get
            {
                return On - Off;
            }
        }
    }
}
