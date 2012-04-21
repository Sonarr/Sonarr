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
        public DateTime SearchTime { get; set; }
        public bool SuccessfulDownload { get; set; }

        [ResultColumn]
        public List<SearchResultItem> SearchResultItems { get; set; }

        [ResultColumn]
        public string SeriesTitle { get; set; }

        [ResultColumn]
        public bool IsDaily { get; set; }

        [ResultColumn]
        public int? EpisodeNumber { get; set; }

        [ResultColumn]
        public string EpisodeTitle { get; set; }

        [ResultColumn]
        public DateTime AirDate { get; set; }

        [ResultColumn]
        public int TotalItems { get; set; }

        [ResultColumn]
        public int Successes { get; set; }
    }
}