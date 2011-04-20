namespace NzbDrone.Core.Providers.Timers
{
    public interface ITimer
    {
        string Name { get; }

        int DefaultInterval { get; }

        void Start();
    }
}