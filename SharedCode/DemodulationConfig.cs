using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis.EDM
{
    [Serializable]
    public class DemodulationConfig
    {
        public List<DetectorExtractSpec> DetectorExtractSpecs = new List<DetectorExtractSpec>();
        public String AnalysisTag = "";
    }
}
