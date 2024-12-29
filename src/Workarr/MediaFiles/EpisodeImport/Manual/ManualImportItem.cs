using Workarr.CustomFormats;
using Workarr.Languages;
using Workarr.Parser.Model;
using Workarr.Qualities;
using Workarr.Tv;

namespace Workarr.MediaFiles.EpisodeImport.Manual
{
    public class ManualImportItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Series Series { get; set; }
        public int? SeasonNumber { get; set; }
        public List<Episode> Episodes { get; set; }
        public int? EpisodeFileId { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string DownloadId { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public int IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public IEnumerable<ImportRejection> Rejections { get; set; }

        public ManualImportItem()
        {
            CustomFormats = new List<CustomFormat>();
        }
    }
}
