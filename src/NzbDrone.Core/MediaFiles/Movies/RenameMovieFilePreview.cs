using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.Movies
{
    public class RenameMovieFilePreview
    {
        public Int32 MovieId { get; set; }
        public Int32 MovieFileId { get; set; }
        public String ExistingPath { get; set; }
        public String NewPath { get; set; }
    }
}
