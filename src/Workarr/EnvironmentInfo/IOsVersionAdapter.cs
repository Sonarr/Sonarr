namespace Workarr.EnvironmentInfo
{
    public interface IOsVersionAdapter
    {
        bool Enabled { get; }
        OsVersionModel Read();
    }
}
