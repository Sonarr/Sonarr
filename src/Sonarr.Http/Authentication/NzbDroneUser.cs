using System.Collections.Generic;
using Nancy.Security;

namespace Sonarr.Http.Authentication
{
    public class NzbDroneUser : IUserIdentity
    {
        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}
