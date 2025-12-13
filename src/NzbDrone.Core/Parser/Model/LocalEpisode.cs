using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedEpisodeInfo FileEpisodeInfo { get; set; }
        public ParsedEpisodeInfo DownloadClientEpisodeInfo { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public ParsedEpisodeInfo FolderEpisodeInfo { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; } = new();
        public List<DeletedEpisodeFile> OldFiles { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; } = new();
        public IndexerFlags IndexerFlags { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string SceneName { get; set; }
        public bool OtherVideoFiles { get; set; }
        public List<CustomFormat> CustomFormats { get; set; } = new();
        public int CustomFormatScore { get; set; }
        public List<CustomFormat> OriginalFileNameCustomFormats { get; set; } = new();
        public int OriginalFileNameCustomFormatScore { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public bool ScriptImported { get; set; }
        public string FileNameBeforeRename { get; set; }
        public string FileNameUsedForCustomFormatCalculation { get; set; }
        public bool ShouldImportExtras { get; set; }
        public List<string> PossibleExtraFiles { get; set; }
        public SubtitleTitleInfo SubtitleInfo { get; set; }

        public int SeasonNumber
        {
            get
            {
                var seasons = Episodes.Select(c => c.SeasonNumber).Distinct().ToList();

                if (seasons.Empty())
                {
                    throw new InvalidSeasonException("Expected one season, but found none");
                }

                if (seasons.Count > 1)
                {
                    throw new InvalidSeasonException("Expected one season, but found {0} ({1})", seasons.Count, string.Join(", ", seasons));
                }

                return seasons.Single();
            }
        }

        public bool IsSpecial => SeasonNumber == 0;

        public override string ToString()
        {
            return Path;
        }

        public string GetSceneOrFileName()
        {
            if (SceneName.IsNotNullOrWhiteSpace())
            {
                return SceneName;
            }

            if (Path.IsNotNullOrWhiteSpace())
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }

            return string.Empty;
        }

        public EpisodeFile ToEpisodeFile()
        {
            var episodeFile = new EpisodeFile
            {
                DateAdded = DateTime.UtcNow,
                SeriesId = Series.Id,
                Path = Path.CleanFilePath(),
                Quality = Quality,
                MediaInfo = MediaInfo,
                Series = Series,
                SeasonNumber = SeasonNumber,
                Episodes = Episodes,
                ReleaseGroup = ReleaseGroup,
                ReleaseHash = ReleaseHash,
                Languages = Languages,
                IndexerFlags = IndexerFlags,
                ReleaseType = ReleaseType,
                SceneName = SceneName,
                OriginalFilePath = GetOriginalFilePath()
            };

            if (Series.Path.IsParentPath(episodeFile.Path))
            {
                episodeFile.RelativePath = Series.Path.GetRelativePath(Path.CleanFilePath());
            }

            if (episodeFile.ReleaseType == ReleaseType.Unknown)
            {
                episodeFile.ReleaseType = DownloadClientEpisodeInfo?.ReleaseType ??
                                          FolderEpisodeInfo?.ReleaseType ??
                                          FileEpisodeInfo.ReleaseType;
            }

            return episodeFile;
        }

        private string GetOriginalFilePath()
        {
            if (FolderEpisodeInfo != null)
            {
                var folderPath = Path.GetAncestorPath(FolderEpisodeInfo.ReleaseTitle);

                if (folderPath != null)
                {
                    return folderPath.GetParentPath().GetRelativePath(Path);
                }
            }

            var parentPath = Path.GetParentPath();
            var grandparentPath = parentPath.GetParentPath();

            if (grandparentPath != null)
            {
                return grandparentPath.GetRelativePath(Path);
            }

            return System.IO.Path.GetFileName(Path);
        }
    }
}
