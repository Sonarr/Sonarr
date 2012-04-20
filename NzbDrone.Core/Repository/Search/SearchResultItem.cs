using System;
using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository.Search
{
    [PrimaryKey("Id", autoIncrement = true)]
    [TableName("SearchResultItems")]
    public class SearchResultItem
    {
        public int Id { get; set; }
        public int SearchResultId { get; set; }
        public string ReportTitle { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public bool Success { get; set; }
        public ReportRejectionType SearchError { get; set; }
    }
}