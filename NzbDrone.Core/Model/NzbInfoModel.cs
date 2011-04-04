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
        public string Title { get; set; }
        public Uri Link { get; set; }

        public bool IsPassworded()
        {
            return Title.EndsWith("(Passworded)", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
