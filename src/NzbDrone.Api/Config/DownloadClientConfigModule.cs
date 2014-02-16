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
                           .SetValidator(rootFolderValidator)
                           .SetValidator(pathExistsValidator)
                           .When(c => !String.IsNullOrWhiteSpace(c.DownloadedEpisodesFolder));
        }
    }
}