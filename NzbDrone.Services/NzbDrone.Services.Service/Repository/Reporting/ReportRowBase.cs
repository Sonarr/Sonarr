using System;
using System.Linq;
using NzbDrone.Common.Contract;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    public abstract class ReportRowBase
    {
        public void LoadBase(ReportBase report)
        {
            Timestamp = DateTime.Now;
            Version = report.Version;
            IsProduction = report.IsProduction;
            UGuid = report.UGuid;
        }

        public string Version { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsProduction { get; set; }

        public Guid UGuid { get; set; }
    }
}
