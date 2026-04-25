using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalReleasePayload
    {
        public string Guid { get; set; }
        public string Title { get; set; }
        public string Indexer { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatPayload> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public long Size { get; set; }
        public string Protocol { get; set; }
        public List<Language> Languages { get; set; }
        public int? Seeders { get; set; }
        public int? Peers { get; set; }
        public int Age { get; set; }
        public int IndexerPriority { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public string InfoUrl { get; set; }
        public string InfoHash { get; set; }
        public bool IsFullSeason { get; set; }
        public string ReleaseType { get; set; }
    }

    public class CustomFormatPayload
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
