using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IFileStationProxy : IDiskStationProxy
    {
        SharedFolderMapping GetSharedFolderMapping(string sharedFolder, DownloadStationSettings settings);

        FileStationListFileInfoResponse GetInfoFileOrDirectory(string path, DownloadStationSettings settings);
    }

    public class FileStationProxy : DiskStationProxyBase, IFileStationProxy
    {
        public FileStationProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.FileStationList, "SYNO.FileStation.List", httpClient, cacheManager, logger)
        {
        }

        public SharedFolderMapping GetSharedFolderMapping(string sharedFolder, DownloadStationSettings settings)
        {
            var info = GetInfoFileOrDirectory(sharedFolder, settings);

            var physicalPath = info.Additional["real_path"].ToString();

            return new SharedFolderMapping(sharedFolder, physicalPath);
        }

        public FileStationListFileInfoResponse GetInfoFileOrDirectory(string path, DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "getinfo", 2);
            requestBuilder.AddQueryParam("path", new[] { path }.ToJson());
            requestBuilder.AddQueryParam("additional", "[\"real_path\"]");

            var response = ProcessRequest<FileStationListResponse>(requestBuilder, $"get info of {path}", settings);

            return response.Data.Files.First();
        }
    }
}
