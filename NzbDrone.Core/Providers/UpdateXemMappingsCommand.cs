using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Providers
{
    public class UpdateXemMappingsCommand : ICommand
    {
        public int? SeriesId { get; private set; }

        public UpdateXemMappingsCommand(int? seriesId)
        {
            SeriesId = seriesId;
        }
    }
}