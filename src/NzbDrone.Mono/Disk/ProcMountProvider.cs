using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Mono.Disk
{
    public interface IProcMountProvider
    {
        List<IMount> GetMounts();
    }

    public class ProcMountProvider : IProcMountProvider
    {
        private const string PROC_MOUNTS_FILENAME = @"/proc/mounts";
        private const string PROC_FILESYSTEMS_FILENAME = @"/proc/filesystems";

        private static readonly Regex OctalRegex = new Regex(@"\\\d{3}", RegexOptions.Compiled);
        private readonly Logger _logger;

        private static string[] _fixedTypes = new[] { "ext3", "ext2", "ext4", "vfat", "fuseblk", "xfs", "jfs", "msdos", "ntfs", "minix", "hfs", "hfsplus", "qnx4", "ufs", "btrfs" };
        private static string[] _networkDriveTypes = new[] { "cifs", "nfs", "nfs4", "nfsd", "sshfs" };

        private static Dictionary<string, bool> _fileSystems;

        public ProcMountProvider(Logger logger)
        {
            _logger = logger;
        }

        public List<IMount> GetMounts()
        {
            try
            {
                if (File.Exists(PROC_MOUNTS_FILENAME))
                {
                    var lines = File.ReadAllLines(PROC_MOUNTS_FILENAME);

                    return lines.Select(ParseLine).OfType<IMount>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to retrieve mounts from {0}", PROC_MOUNTS_FILENAME);
            }

            return new List<IMount>();
        }

        private Dictionary<string, bool> GetFileSystems()
        {
            if (_fileSystems == null)
            {
                var result = new Dictionary<string, bool>();
                try
                {
                    if (File.Exists(PROC_FILESYSTEMS_FILENAME))
                    {
                        var lines = File.ReadAllLines(PROC_FILESYSTEMS_FILENAME);

                        foreach (var line in lines)
                        {
                            var split = line.Split('\t');

                            result.Add(split[1], split[0] != "nodev");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to get filesystem types from {0}, using default set.", PROC_FILESYSTEMS_FILENAME);
                }

                if (result.Empty())
                {
                    foreach (var type in _fixedTypes)
                    {
                        result.Add(type, true);
                    }
                }

                _fileSystems = result;
            }

            return _fileSystems;
        }

        private IMount ParseLine(string line)
        {
            var split = line.Split(' ').Select(ExpandEscapes).ToArray();

            if (split.Length != 6)
            {
                _logger.Debug("Unable to parse {0} line: {1}", PROC_MOUNTS_FILENAME, line);
                return null;
            }

            var name = split[0];
            var mount = split[1];
            var type = split[2];
            var options = ParseOptions(split[3]);

            if (!mount.StartsWith("/"))
            {
                // Possible a namespace like 'net:[1234]'.
                // But we just filter anything not starting with /, we're not interested in anything that isn't mounted somewhere.
                return null;
            }

            var driveType = FindDriveType.Find(type);

            if (name.StartsWith("/dev/") || GetFileSystems().GetValueOrDefault(type, false))
            {
                // Not always fixed, but lets assume it.
                driveType = DriveType.Fixed;
            }

            if (_networkDriveTypes.Contains(type))
            {
                driveType = DriveType.Network;
            }

            return new ProcMount(driveType, name, mount, type, new MountOptions(options));
        }

        private Dictionary<string, string> ParseOptions(string options)
        {
            var result = new Dictionary<string, string>();

            foreach (var option in options.Split(','))
            {
                var split = option.Split(new[] { '=' }, 2);

                result.Add(split[0], split.Length == 2 ? split[1] : string.Empty);
            }

            return result;
        }

        private string ExpandEscapes(string mount)
        {
            return OctalRegex.Replace(mount, match => match.Captures[0].Value.FromOctalString());
        }
    }
}
