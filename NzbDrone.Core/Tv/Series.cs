using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;


namespace NzbDrone.Core.Tv
{
    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public String AirTime { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public BacklogSettingType BacklogSetting { get; set; }
        public string Network { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }

        public int RootFolderId { get; set; }
        public string FolderName { get; set; }
        public LazyLoaded<RootFolder> RootFolder { get; set; }

        //Todo: Use this to auto link RootFolder and Folder (using the proper path separator)
        public string Path
        {
            get
            {
                if (RootFolder == null || RootFolder.Value == null || String.IsNullOrWhiteSpace(RootFolder.Value.Path))
                {
                    return null;
                }

                return System.IO.Path.Combine(RootFolder.Value.Path, FolderName);
            }
        }

        public DateTime? FirstAired { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
    }
}