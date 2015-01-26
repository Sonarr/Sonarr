using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Authentication
{
    public class User : ModelBase
    {
        public Guid Identifier { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
