using NzbDrone.Core.Movies;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMoviesInfo
    {
        Movie GetMovieInfo(int tmdbMovieId);
    }
}