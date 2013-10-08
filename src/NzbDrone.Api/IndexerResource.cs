using System;
using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.REST;

namespace NzbDrone.Api
{
    public class ProviderResource : RestResource
    {
        public Boolean Enable { get; set; }
        public String Name { get; set; }
        public List<Field> Fields { get; set; }
        public String Implementation { get; set; }
        public String ConfigContract { get; set; }
    }
}