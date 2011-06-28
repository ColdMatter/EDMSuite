using System;
using System.Collections.Generic;
using System.Text;

namespace SympatheticHardwareControl
{
    public interface HardwareReportable
    {
        Dictionary<String, Object> GetHardwareReport();
    }
}
