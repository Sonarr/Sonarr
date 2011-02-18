using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Model
{
    public class NzbInfoModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string TitleFix { get; set; }
        public NzbSiteModel Site { get; set; }
        public Uri Link { get; set; }
        public string Description { get; set; }
        public bool Proper { get; set; }
        public QualityTypes Quality { get; set; }

        public bool IsPassworded()
        {
            return Title.EndsWith("(Passworded)", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
