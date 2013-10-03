using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Processes
{
    public class ProcessOutput
    {
        public List<String> Standard { get; set; }
        public List<String> Error { get; set; }

        public ProcessOutput()
        {
            Standard = new List<string>();
            Error = new List<string>();
        }
    }
}
