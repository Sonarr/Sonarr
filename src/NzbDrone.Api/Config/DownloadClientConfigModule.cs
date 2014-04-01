using System;
using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigModule : NzbDroneConfigModule<DownloadClientConfigResource>
    {
        public DownloadClientConfigModule(IConfigService configService, RootFolderValidator rootFolderValidator, PathExistsValidator pathExistsValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.DownloadedEpisodesFolder)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(pathExistsValidator)
                           .When(c => !String.IsNullOrWhiteSpace(c.DownloadedEpisodesFolder));

            SharedValidator.RuleFor(c => c.BlacklistGracePeriod)
                           .InclusiveBetween(1, 24);

            SharedValidator.RuleFor(c => c.BlacklistRetryInterval)
                           .InclusiveBetween(5, 120);

            SharedValidator.RuleFor(c => c.BlacklistRetryLimit)
                           .InclusiveBetween(0, 10);
        }
    }
}