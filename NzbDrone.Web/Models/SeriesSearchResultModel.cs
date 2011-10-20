using System;

namespace NzbDrone.Web.Models
{
    public class SeriesSearchResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime FirstAired { get; set; }
    }
}