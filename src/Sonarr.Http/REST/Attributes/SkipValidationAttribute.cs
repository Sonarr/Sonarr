using System;

namespace Sonarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipValidationAttribute : Attribute
    {
        public SkipValidationAttribute(bool skip = true, bool skipShared = true)
        {
            Skip = skip;
            SkipShared = skipShared;
        }

        public bool Skip { get; }
        public bool SkipShared { get;  }
    }
}
