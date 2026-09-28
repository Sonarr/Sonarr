using FluentValidation.Validators;
using NzbDrone.Core.Authentication;

namespace NzbDrone.Core.Validation
{
    public class OidcAuthorityValidator : PropertyValidator
    {
        private readonly IOidcDiscoveryService _oidcDiscoveryService;

        public OidcAuthorityValidator(IOidcDiscoveryService oidcDiscoveryService)
        {
            _oidcDiscoveryService = oidcDiscoveryService;
        }

        protected override string GetDefaultMessageTemplate() => "Unable to retrieve the OIDC configuration from the Authority";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            return _oidcDiscoveryService.IsDiscoverable(context?.PropertyValue as string);
        }
    }
}
