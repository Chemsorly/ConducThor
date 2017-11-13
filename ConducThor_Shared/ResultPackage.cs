using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared
{
    public class ResultPackage
    {
        public WorkPackage WorkPackage { get; set; }
        public byte[] ModelFile { get; set; }
        public byte[] PredictionFile { get; set; }
    }
}
