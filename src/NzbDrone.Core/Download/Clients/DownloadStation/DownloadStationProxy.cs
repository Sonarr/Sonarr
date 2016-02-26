using FluentValidation.Results;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public interface IDownloadStationProxy
    {
        void Test(List<ValidationFailure> failures, DownloadStationSettings settings);
        IEnumerable<DownloadClientItem> GetItems(DownloadStationSettings settings);
        DownloadClientStatus GetStatus(DownloadStationSettings settings);
        void RemoveItem(string downloadId, bool deleteData, DownloadStationSettings settings);
        string AddFromUrl(RemoteEpisode remoteEpisode, string url, DownloadStationSettings settings);
        string AddFromFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent, DownloadStationSettings settings);
    }

    public class DownloadStationProxy : IDownloadStationProxy
    {
        public string AddFromFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public string AddFromUrl(RemoteEpisode remoteEpisode, string url, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DownloadClientItem> GetItems(DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public DownloadClientStatus GetStatus(DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(string downloadId, bool deleteData, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public void Test(List<ValidationFailure> failures, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
