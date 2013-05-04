namespace NzbDrone.Owin
{
    public interface IHostController
    {
        string AppUrl { get; }
        void StartServer();
        void RestartServer();
        void StopServer();
    }
}