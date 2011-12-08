using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    public interface ExperimentReportable
    {
        Dictionary<String, Object> GetExperimentReport();
    }
}
