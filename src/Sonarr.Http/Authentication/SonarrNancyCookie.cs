using System;
using Nancy.Cookies;

namespace Sonarr.Http.Authentication
{
    public class SonarrNancyCookie : NancyCookie
    {
        public SonarrNancyCookie(string name, string value)
            : base(name, value)
        {
        }

        public SonarrNancyCookie(string name, string value, DateTime expires)
            : base(name, value, expires)
        {
        }

        public SonarrNancyCookie(string name, string value, bool httpOnly)
            : base(name, value, httpOnly)
        {
        }

        public SonarrNancyCookie(string name, string value, bool httpOnly, bool secure)
            : base(name, value, httpOnly, secure)
        {
        }

        public SonarrNancyCookie(string name, string value, bool httpOnly, bool secure, DateTime? expires)
            : base(name, value, httpOnly, secure, expires)
        {
        }

        public override string ToString()
        {
            return base.ToString() + "; SameSite=Lax";
        }
    }
}
