using System.Linq;

namespace NzbDrone.Api.RequestModels
{
    public class QualityProfileRequest : IApiRequest
    {
        public string ApiKey { get; set; }
        public int Id { get; set; }
    }
}