namespace NzbDrone.Common.EnvironmentInfo
{
    public class OsVersionModel
    {
        public OsVersionModel(string name, string version, string fullName = null)
        {
            Name = Trim(name);
            Version = Trim(version);

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = $"{Name} {Version}";
            }

            FullName = Trim(fullName);
        }

        private static string Trim(string source)
        {
            return source.Trim().Trim('"', '\'');
        }

        public string Name { get; }
        public string FullName { get; }
        public string Version { get; }
    }
}
