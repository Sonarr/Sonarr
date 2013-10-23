using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistService
    {
        bool Blacklisted(string sourceTitle);
    }

    public class BlacklistService : IBlacklistService, IHandle<DownloadFailedEvent>
    {
        private readonly IBlacklistRepository _blacklistRepository;

        public BlacklistService(IBlacklistRepository blacklistRepository)
        {
            _blacklistRepository = blacklistRepository;
        }

        public bool Blacklisted(string sourceTitle)
        {
            return _blacklistRepository.Blacklisted(sourceTitle);
        }

        public void Handle(DownloadFailedEvent message)
        {
            var blacklist = new Blacklist
                            {
                                SeriesId = message.Series.Id,
                                EpisodeId = message.Episode.Id,
                                SourceTitle = message.SourceTitle,
                                Quality = message.Quality,
                                Date = DateTime.UtcNow
                            };

            _blacklistRepository.Insert(blacklist);
        }
    }
}
