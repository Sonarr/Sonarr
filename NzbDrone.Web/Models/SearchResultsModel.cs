using System;

namespace NzbDrone.Web.Models
{
    public class SearchResultsModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string SearchTime { get; set; }
        public int ReportCount { get; set; }
        public bool Successful { get; set; }
    }
}