using Owin;

namespace NzbDrone.Owin.MiddleWare
{
    public interface IOwinMiddleWare
    {
        void Attach(IAppBuilder appBuilder);
    }
}