using FluentValidation;

namespace NzbDrone.Api.REST
{
    public abstract class RestResource
    {
        public int Id { get; set; }

        public virtual string ResourceName
        {
            get
            {
                return GetType().Name.ToLower().Replace("resource", "");
            }
        }
    }
}