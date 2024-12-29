using System.Net;
using Workarr.Exceptions;

namespace Workarr.Profiles.Qualities
{
    public class QualityProfileInUseException : WorkarrClientException
    {
        public QualityProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "QualityProfile [{0}] is in use.", name)
        {
        }
    }
}
