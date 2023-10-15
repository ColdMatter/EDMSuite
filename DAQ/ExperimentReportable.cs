using System;
using System.Collections.Generic;

namespace DAQ
{
    public interface ExperimentReportable
    {
        Dictionary<String, Object> GetExperimentReport();
    }
}
