using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IFileStationProxy
    {
        SharedFolderMapping GetSharedFolderMapping(string sharedFolder, DownloadStationSettings settings);
        IEnumerable<int> GetApiVersion(DownloadStationSettings settings);
        FileStationListFileInfoResponse GetInfoFileOrDirectory(string path, DownloadStationSettings settings);
    }

    public class FileStationProxy : DiskStationProxyBase, IFileStationProxy
    {
        public FileStationProxy(IHttpClient httpClient, Logger logger)
            : base(httpClient, logger)
        {
        }

        public IEnumerable<int> GetApiVersion(DownloadStationSettings settings)
        {
            return base.GetApiVersion(settings, DiskStationApi.FileStationList);
        }

        public SharedFolderMapping GetSharedFolderMapping(string sharedFolder, DownloadStationSettings settings)
        {
            var info = GetInfoFileOrDirectory(sharedFolder, settings);

            var physicalPath = info.Additional["real_path"].ToString();

            return new SharedFolderMapping(sharedFolder, physicalPath);
        }

        public FileStationListFileInfoResponse GetInfoFileOrDirectory(string path, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>
            {
                { "api", "SYNO.FileStation.List" },
                { "version", "2" },
                { "method", "getinfo" },
                { "path", new [] { path }.ToJson() },
                { "additional", $"[\"real_path\"]" }
            };

            var response = ProcessRequest<FileStationListResponse>(DiskStationApi.FileStationList, arguments, settings, $"get info of {path}");

            return response.Data.Files.First();
        }
    }
}
