using System;
using System.Collections.Generic;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class EpisodeFile
    {
        [SubSonicPrimaryKey]
        public virtual int FileId { get; set; }
        public int SeriesId { get; set; }
        public string Path { get; set; }
        public QualityTypes Quality { get; set; }
        public bool Proper { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }

        [SubSonicToManyRelation]
        public virtual List<Episode> Episodes { get; private set; }
    }
}
