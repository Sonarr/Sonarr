using System.Threading.Tasks;

namespace NzbDrone.SignalR
{
    public interface IBroadcastSignalRMessage
    {
        bool IsConnected { get; }
        Task BroadcastMessage(SignalRMessage message);
    }
}
