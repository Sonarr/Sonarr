using FluentValidation.Results;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationUsenetClient : UsenetClientBase<DownloadStationSettings>
    {
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            throw new NotImplementedException();
        }

        public override DownloadClientStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            throw new NotImplementedException();
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            throw new NotImplementedException();
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            throw new NotImplementedException();
        }
    }
}
