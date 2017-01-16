using System.IO;
using System.Reflection;

namespace NzbDrone.Common.Extensions
{
    public static class ResourceExtensions
    {
        public static byte[] GetManifestResourceBytes(this Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);

            var result = new byte[stream.Length];
            var read = stream.Read(result, 0, result.Length);

            if (read != result.Length)
            {
                throw new EndOfStreamException("Reached end of stream before reading enough bytes.");
            }

            return result;
        }
    }
}
