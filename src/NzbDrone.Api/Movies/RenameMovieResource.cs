using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Movies
{
    public class RenameMovieResource : RestResource
    {
        public Int32 MovieId { get; set; }
        public Int32 MovieFileId { get; set; }
        public String ExistingPath { get; set; }
        public String NewPath { get; set; }
    }
}
