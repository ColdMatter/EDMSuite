using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlFHardwareControl
{
    public struct ResourceCondition
    {
        public string Comparison;
        public string Value;
        public string Resource;

        public ResourceCondition(string _res, string _comp, string _val)
        {
            Resource = _res;
            Comparison = _comp;
            Value = _val;
        }
    }
}
