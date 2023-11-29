using FluentValidation.Validators;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Validation
{
    public class DownloadClientExistsValidator : PropertyValidator
    {
        private readonly IDownloadClientFactory _downloadClientFactory;

        public DownloadClientExistsValidator(IDownloadClientFactory downloadClientFactory)
        {
            _downloadClientFactory = downloadClientFactory;
        }

        protected override string GetDefaultMessageTemplate() => "Download Client does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context?.PropertyValue == null || (int)context.PropertyValue == 0)
            {
                return true;
            }

            return _downloadClientFactory.Exists((int)context.PropertyValue);
        }
    }
}
