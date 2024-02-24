using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class ImportListItemInfo : ModelBase
    {
        public ImportListItemInfo()
        {
            Seasons = new List<Season>();
        }

        public int ImportListId { get; set; }
        public string ImportList { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int TvdbId { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public int MalId { get; set; }
        public int AniListId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Season> Seasons { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", ReleaseDate, Title);
        }
    }
}
