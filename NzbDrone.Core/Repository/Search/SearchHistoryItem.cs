using System;
using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Repository.Search
{
    [PrimaryKey("Id", autoIncrement = true)]
    [TableName("SearchHistoryItems")]
    public class SearchHistoryItem
    {
        public int Id { get; set; }
        public int SearchHistoryId { get; set; }
        public string ReportTitle { get; set; }
        public string Indexer { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public bool Success { get; set; }
        public ReportRejectionType SearchError { get; set; }
        public QualityTypes Quality { get; set; }
        public bool Proper { get; set; }
        public int Age { get; set; }
        public LanguageType Language { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}", ReportTitle, Quality, SearchError);
        }
    }
}