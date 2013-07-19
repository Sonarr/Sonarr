using NzbDrone.Api.SignalR;

namespace NzbDrone.Api.Series
{
    public class SeriesConnection : BasicResourceConnection<Core.Tv.Series>
    {
        public override string Resource
        {
            get { return "/Series"; }
        }
    }
}
