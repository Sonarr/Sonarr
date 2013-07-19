using System.Collections.Generic;
using Nancy.Security;

namespace NzbDrone.Api.Authentication
{
    public class NzbDroneUser : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}
