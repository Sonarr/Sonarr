using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchCommand : ICommand
    {
        public int SeriesId { get; set; }
    }
}