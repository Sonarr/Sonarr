using FluentValidation;

namespace NzbDrone.Api.REST
{
    public abstract class RestResource<T>
        where T : RestResource<T>, new()
    {

        public int Id { get; set; }

        public virtual string ResourceName
        {
            get
            {
                return GetType().Name.ToLower();
            }
        }

        protected AbstractValidator<T> PostValidator { get; private set; }
        protected AbstractValidator<T> PutValidator { get; private set; }

        public RestResource()
        {
            PostValidator = new InlineValidator<T>();
            PutValidator = new InlineValidator<T>();

            PostValidator.RuleFor(m => m.Id).Equal(0);
            PutValidator.RuleFor(m => m.Id).GreaterThan(0);
        }


        public void ValidateForPost()
        {
            PostValidator.ValidateAndThrow((T)this);
        }

        public void ValidateForPut()
        {
            PutValidator.ValidateAndThrow((T)this);
        }
    }
}