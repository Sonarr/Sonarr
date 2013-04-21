using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Api.Mapping
{
    public class ResourceMappingException : ApplicationException
    {
        public ResourceMappingException(IEnumerable<string> error)
            : base(Environment.NewLine + String.Join(Environment.NewLine, error.OrderBy(c => c)))
        {

        }
    }
}