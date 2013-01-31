using System.Linq;

namespace NzbDrone.Core.Tvdb
{
    public class TvdbAsyncResult <T>
    {
        public T Data { get; set; }
        public object UserState { get; set; }
    }
}
