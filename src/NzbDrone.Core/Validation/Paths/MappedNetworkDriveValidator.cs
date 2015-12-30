using System;
using System.IO;
using System.Text.RegularExpressions;
using FluentValidation.Validators;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation.Paths
{
    public class MappedNetworkDriveValidator : PropertyValidator
    {
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IDiskProvider _diskProvider;

        private static readonly Regex DriveRegex = new Regex(@"[a-z]\:\\", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MappedNetworkDriveValidator(IRuntimeInfo runtimeInfo, IDiskProvider diskProvider)
            : base("Mapped Network Drive and Windows Service")
        {
            _runtimeInfo = runtimeInfo;
            _diskProvider = diskProvider;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;
            if (OsInfo.IsNotWindows) return true;
            if (!_runtimeInfo.IsWindowsService) return true;

            var path = context.PropertyValue.ToString();

            if (!DriveRegex.IsMatch(path)) return true;
            
            var mount = _diskProvider.GetMount(path);

            if (mount != null && mount.DriveType == DriveType.Network)
            {
                return false;
            }

            return true;
        }
    }
}