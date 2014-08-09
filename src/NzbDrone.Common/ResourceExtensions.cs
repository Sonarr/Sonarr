using NzbDrone.Common.EnsureThat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NzbDrone.Common
{
    public static class ResourceExtensions
    {
        public static Byte[] GetManifestResourceBytes(this Assembly assembly, String name)
        {
            var stream = assembly.GetManifestResourceStream(name);

            var result = new Byte[stream.Length];
            var read = stream.Read(result, 0, result.Length);

            if (read != result.Length)
            {
                throw new EndOfStreamException("Reached end of stream before reading enough bytes.");
            }

            return result;
        }
    }
}
