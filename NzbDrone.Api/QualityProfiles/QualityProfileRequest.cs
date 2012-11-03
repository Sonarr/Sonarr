using System.Linq;

namespace NzbDrone.Api.QualityProfiles
{
    public class QualityProfileRequest : IApiRequest
    {
        public string ApiKey { get; set; }
        public int Id { get; set; }
    }
}