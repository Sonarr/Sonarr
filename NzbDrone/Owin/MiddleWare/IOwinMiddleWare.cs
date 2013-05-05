using Owin;

namespace NzbDrone.Owin.MiddleWare
{
    public interface IOwinMiddleWare
    {
        int Order { get; }
        void Attach(IAppBuilder appBuilder);
    }
}