using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class SeasonModel
    {
        public int SeasonNumber { get; set; }
        public List<EpisodeModel> Episodes { get; set; }
    }
}