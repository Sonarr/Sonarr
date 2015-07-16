using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Download.Clients.RTorrent;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.rTorrent
{
    public interface IRTorrentDirectoryValidator
    {
        ValidationResult Validate(RTorrentSettings instance);
    }

    public class RTorrentDirectoryValidator : AbstractValidator<RTorrentSettings>, IRTorrentDirectoryValidator
    {
        public RTorrentDirectoryValidator(RootFolderValidator rootFolderValidator,
                                          PathExistsValidator pathExistsValidator,
                                          DroneFactoryValidator droneFactoryValidator,
                                          MappedNetworkDriveValidator mappedNetworkDriveValidator)
        {
            RuleFor(c => c.TvDirectory).Cascade(CascadeMode.StopOnFirstFailure)
                                       .IsValidPath()
                                       .SetValidator(rootFolderValidator)
                                       .SetValidator(droneFactoryValidator)
                                       .SetValidator(mappedNetworkDriveValidator)
                                       .SetValidator(pathExistsValidator)
                                       .When(c => c.Host == "localhost" || c.Host == "127.0.0.1");
        }
    }
}
