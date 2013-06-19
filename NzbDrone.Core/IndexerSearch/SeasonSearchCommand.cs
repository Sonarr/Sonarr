using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchCommand : ICommand
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
    }
}