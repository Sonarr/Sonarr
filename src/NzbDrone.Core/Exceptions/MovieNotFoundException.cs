using System;
using NzbDrone.Common.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Exceptions
{
    public class MovieNotFoundException : NzbDroneException
    {
        public int TmdbmovieId { get; set; }

        public MovieNotFoundException(int tmdbMovieId)
            : base(string.Format("Movie with tmdbid {0} was not found, it may have been removed from TheTMDB.", tmdbMovieId))
        {
            TmdbmovieId = tmdbMovieId;
        }

        public MovieNotFoundException(int tmdbMovieId, string message, params object[] args)
            : base(message, args)
        {
            TmdbmovieId = tmdbMovieId;
        }

        public MovieNotFoundException(int tmdbMovieId, string message)
            : base(message)
        {
            TmdbmovieId = tmdbMovieId;
        }
    }
}
