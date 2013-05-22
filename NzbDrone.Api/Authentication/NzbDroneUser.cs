using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Security;

namespace NzbDrone.Api.Authentication
{
    public class NzbDroneUser : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}
