using System;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.Repository.Search
{
    public class SearchHistoryItem
    {
        public int Id { get; set; }
        public int SearchHistoryId { get; set; }
        public string ReportTitle { get; set; }
        public string Indexer { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public bool Success { get; set; }
        public ReportRejectionReasons SearchError { get; set; }
        public Quality Quality { get; set; }
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