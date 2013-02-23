namespace NzbDrone.Core.Tv.Events
{
    public class SeriesAddedEvent
    {
        public Series Series { get; private set; }

        public SeriesAddedEvent(Series series)
        {
            Series = series;
        }
    }
}