using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles.Movies;

namespace NzbDrone.Api.Movies
{
    public class RenameMovieModule : NzbDroneRestModule<RenameMovieResource>
    {
        private readonly IRenameMovieFileService _renameMovieFileService;

        public RenameMovieModule(IRenameMovieFileService renameMovieFileService)
            : base("renamemovie")
        {
            _renameMovieFileService = renameMovieFileService;

            GetResourceAll = GetMovies;
        }

        private List<RenameMovieResource> GetMovies()
        {
            int movieId;

            if (Request.Query.MovieId.HasValue)
            {
                movieId = (int)Request.Query.MovieId;
            }

            else
            {
                throw new BadRequestException("movieId is missing");
            }


            return ToListResource(() => _renameMovieFileService.GetRenamePreviews(movieId));
        }
    }
}
