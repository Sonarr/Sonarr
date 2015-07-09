namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public class HadoukenResponse<TResult>
    {
        public int Id { get; set; }
        public TResult Result { get; set; }
        public HadoukenError Error { get; set; }
    }
}
