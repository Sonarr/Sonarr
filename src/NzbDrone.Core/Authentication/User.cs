using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Authentication
{
    public enum UserRole
    {
        Admin,
        ReadOnly
    }

    public class User : ModelBase
    {
        public Guid Identifier { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public int Iterations { get; set; }
        public UserRole Role { get; set; }

        public string ApiKey { get; set; }
    }
}
