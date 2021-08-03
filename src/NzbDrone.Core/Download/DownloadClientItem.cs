using System;
using System.Diagnostics;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    [DebuggerDisplay("{DownloadClientInfo?.Name}:{Title}")]
    public class DownloadClientItem
    {
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public double? SeedRatio { get; set; }
        public OsPath OutputPath { get; set; }
        public string Message { get; set; }
        public DownloadItemStatus Status { get; set; }
        public bool IsEncrypted { get; set; }
        public bool CanMoveFiles { get; set; }
        public bool CanBeRemoved { get; set; }
        public bool Removed { get; set; }

        public DownloadClientItem Clone()
        {
            return MemberwiseClone() as DownloadClientItem;
        }
    }

    public class DownloadClientItemClientInfo
    {
        public DownloadProtocol Protocol { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        public static DownloadClientItemClientInfo FromDownloadClient<TSettings>(
            DownloadClientBase<TSettings> downloadClient)
            where TSettings : IProviderConfig, new()
        {
            return new DownloadClientItemClientInfo
            {
                Protocol = downloadClient.Protocol,
                Type = downloadClient.Name,
                Id = downloadClient.Definition.Id,
                Name = downloadClient.Definition.Name
            };
        }
    }
}
