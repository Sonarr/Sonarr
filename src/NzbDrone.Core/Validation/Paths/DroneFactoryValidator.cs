using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Validation.Paths
{
    public class DroneFactoryValidator : PropertyValidator
    {
        private readonly IConfigService _configService;

        public DroneFactoryValidator(IConfigService configService)
            : base("Path is already used for drone factory")
        {
            _configService = configService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return false;

            var droneFactory = _configService.DownloadedEpisodesFolder;

            if (string.IsNullOrWhiteSpace(droneFactory)) return true;

            return !droneFactory.PathEquals(context.PropertyValue.ToString());
        }
    }
}