using System.Linq;

namespace NzbDrone.Web.Models
{
    public class TvDbSearchResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Banner { get; set; }
        public string DisplayedTitle { get; set; }
        public string Url { get; set; }
    }
}