using Workarr.Datastore;

namespace Workarr.Authentication
{
    public class User : ModelBase
    {
        public Guid Identifier { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public int Iterations { get; set; }
    }
}
