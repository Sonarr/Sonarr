using System;
using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository.Search
{
    [PrimaryKey("Id", autoIncrement = true)]
    [TableName("SearchResults")]
    public class SearchResult
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeId { get; set; }
        public DateTime? AirDate { get; set; }
        public DateTime SearchTime { get; set; }

        [ResultColumn]
        public List<SearchResultItem> SearchResultItems { get; set; }
    }
}