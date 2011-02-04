using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class MappingModel
    {
        public int Id { get; set; }
        public String Path { get; set; }
    }
}