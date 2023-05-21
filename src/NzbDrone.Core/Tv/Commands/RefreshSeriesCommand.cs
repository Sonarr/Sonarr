using System.Collections.Generic;
using System.Text.Json.Serialization;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int SeriesId
        {
            get => 0;
            set
            {
                if (SeriesIds.Empty())
                {
                    SeriesIds.Add(value);
                }
            }
        }

        public List<int> SeriesIds { get; set; }
        public bool IsNewSeries { get; set; }

        public RefreshSeriesCommand()
        {
            SeriesIds = new List<int>();
        }

        public RefreshSeriesCommand(List<int> seriesIds, bool isNewSeries = false)
        {
            SeriesIds = seriesIds;
            IsNewSeries = isNewSeries;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => SeriesIds.Empty();

        public override bool IsLongRunning => true;
    }
}
