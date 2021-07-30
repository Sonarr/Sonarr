namespace NzbDrone.Host
{
    public interface IHostController
    {
        void StartServer();
        void StopServer();
    }
}
