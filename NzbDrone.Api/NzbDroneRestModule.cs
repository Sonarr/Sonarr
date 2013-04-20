using NzbDrone.Api.REST;
using NzbDrone.Api.Validation;

namespace NzbDrone.Api
{
    public abstract class NzbDroneRestModule<TResource> : RestModule<TResource> where TResource : RestResource, new()
    {
        protected NzbDroneRestModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
            PostValidator.RuleFor(r => r.Id).IsZero();
            PutValidator.RuleFor(r => r.Id).ValidId();
        }

    }
}