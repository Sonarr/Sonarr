using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class FeedInfoModel
    {
        public FeedInfoModel(string name, string url)
        {
            Name = name ?? "UN-NAMED";
            Url = ParseUrl(url);
        }

        public string Name { get; private set; }
        public string Url { get; private set; }

        private static string ParseUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {   
                uri = new Uri(new Uri(CentralDispatch.ExecutablePath + Path.DirectorySeparatorChar), url);
            }
            return uri.IsFile ? uri.AbsolutePath.Replace("%20", " ") : uri.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FeedInfoModel)) return false;
            return Equals((FeedInfoModel) obj);
        }

        public bool Equals(FeedInfoModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.Url, Url);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Url != null ? Url.GetHashCode() : 0);
            }
        }
    }
}
