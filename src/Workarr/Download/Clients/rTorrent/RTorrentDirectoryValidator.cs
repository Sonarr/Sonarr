using FluentValidation;
using FluentValidation.Results;
using Workarr.Extensions;
using Workarr.Validation.Paths;

namespace Workarr.Download.Clients.rTorrent
{
    public interface IRTorrentDirectoryValidator
    {
        ValidationResult Validate(RTorrentSettings instance);
    }

    public class RTorrentDirectoryValidator : AbstractValidator<RTorrentSettings>, IRTorrentDirectoryValidator
    {
        public RTorrentDirectoryValidator(RootFolderValidator rootFolderValidator,
                                          PathExistsValidator pathExistsValidator,
                                          MappedNetworkDriveValidator mappedNetworkDriveValidator)
        {
            RuleFor(c => c.TvDirectory).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                       .SetValidator(rootFolderValidator)
                                       .SetValidator(mappedNetworkDriveValidator)
                                       .SetValidator(pathExistsValidator)
                                       .When(c => c.TvDirectory.IsNotNullOrWhiteSpace())
                                       .When(c => c.Host == "localhost" || c.Host == "127.0.0.1");
        }
    }
}
