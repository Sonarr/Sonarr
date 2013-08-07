using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public interface IOwinMiddleWare
    {
        int Order { get; }
        void Attach(IAppBuilder appBuilder);
    }
}