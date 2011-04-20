using System;
using System.Collections.Generic;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Model
{
    public class EpisodeParseResult
    {
        internal string SeriesTitle { get; set; }
        public int SeriesId { get; set; }

        internal int SeasonNumber { get; set; }
        internal List<int> Episodes { get; set; }
        internal int Year { get; set; }

        internal DateTime AirDate { get; set; }

        public bool Proper { get; set; }

        public QualityTypes Quality { get; set; }

        public override string ToString()
        {
            if (Episodes == null)
                return string.Format("Series:{0} Air Date:{1}", SeriesTitle, AirDate.Date);

            return string.Format("Series:{0} Season:{1} Episode:{2}", SeriesTitle, SeasonNumber,
                                 String.Join(",", Episodes));

        }
    }
}