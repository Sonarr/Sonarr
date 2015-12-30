using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using Mono.Unix;

namespace NzbDrone.Mono
{
    public interface IProcMountProvider
    {
        List<IMount> GetMounts();
    }

    public class ProcMountProvider : IProcMountProvider
    {
        private static string[] _fixedTypes = new [] { "ext3", "ext2", "ext4", "vfat", "fuseblk", "xfs", "jfs", "msdos", "ntfs", "minix", "hfs", "hfsplus", "qnx4", "ufs", "btrfs" };
        private static string[] _networkDriveTypes = new [] { "cifs", "nfs", "nfs4", "nfsd", "sshfs" };

        private static Dictionary<string, bool> _fileSystems;

        private readonly Logger _logger;

        public ProcMountProvider(Logger logger)
        {
            _logger = logger;
        }

        public List<IMount> GetMounts()
        {
            try
            {
                if (File.Exists(@"/proc/mounts"))
                {
                    var lines = File.ReadAllLines(@"/proc/mounts");

                    return lines.Select(ParseLine).OfType<IMount>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.DebugException("Failed to retrieve mounts from /proc/mounts", ex);
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
                    if (File.Exists(@"/proc/filesystems"))
                    {
                        var lines = File.ReadAllLines(@"/proc/filesystems");

                        foreach (var line in lines)
                        {
                            var split = line.Split('\t');

                            result.Add(split[1], split[0] != "nodev");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.DebugException("Failed to get filesystem types from /proc/filesystems, using default set.", ex);
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
            var split = line.Split(' ');

            if (split.Length != 6)
            {
                _logger.Debug("Unable to parser /proc/mount line: {0}", line);
            }

            var name = split[0];
            var mount = split[1];
            var type = split[2];
            var options = ParseOptions(split[3]);

            var driveType = DriveType.Unknown;
            
            if (name.StartsWith("/dev/") || GetFileSystems().GetValueOrDefault(type, false))
            {
                // Not always fixed, but lets assume it.
                driveType = DriveType.Fixed;
            }

            if (_networkDriveTypes.Contains(type))
            {
                driveType = DriveType.Network;
            }

            if (type == "zfs")
            {
                driveType = DriveType.Fixed;
            }

            return new ProcMount(driveType, name, mount, type, options);
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
    }
}
