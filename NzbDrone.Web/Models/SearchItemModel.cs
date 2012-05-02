using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class SearchItemModel
    {
        public int Id { get; set; }
        public string ReportTitle { get; set; }
        public string Indexer { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public bool Success { get; set; }
        public string SearchError { get; set; }
        public string Quality { get; set; }
        public int QualityInt { get; set; }
        public bool Proper { get; set; }
        public int Age { get; set; }
        public string Language { get; set; }
        public string Size { get; set; }
        public string Details { get; set; }
    }
}