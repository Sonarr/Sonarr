using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchCommand : ICommand
    {
        public int EpisodeId { get; set; }
    }
}