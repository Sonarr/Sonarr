using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        Movie FindByTitle(string cleanTitle);
        Movie FindByTitle(string cleanTitle, int year);
        Movie FindByImdbId(string ImdbId);
        Movie FindByTmdbId(string TmbdId);
        bool MoviePathExists(string path);
        Movie GetMovieByFileId(int fileId);
        void SetFileId(int movieId, int fileId);
        PagingSpec<Movie> MoviesWithoutFile(PagingSpec<Movie> pagingSpec);

    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        public MovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        { }

        public Movie FindByTitle(string cleanTitle)
        {
            return Query.SingleOrDefault(s =>
                s.CleanTitle.Equals(cleanTitle, StringComparison.InvariantCultureIgnoreCase) ||
                s.OriginalTitle.CleanMovieTitle().Equals(cleanTitle, StringComparison.InvariantCultureIgnoreCase));
        }

        public Movie FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle || s.OriginalTitle.CleanMovieTitle() == cleanTitle)
                        .AndWhere(s => s.Year == year)
                        .SingleOrDefault();
        }


        public Movie FindByImdbId(string ImdbId)
        {
            return Query.SingleOrDefault(s=>s.ImdbId.Equals(ImdbId));
        }

        public Movie FindByTmdbId(string TmbdId)
        {
            return Query.SingleOrDefault(s=>s.TmdbId.Equals(TmbdId));
        }

        public Movie GetMovieByFileId(int fileId)
        {
            return Query.SingleOrDefault(s => s.MovieFileId == fileId);
        }

        public bool MoviePathExists(string path)
        {
            return Query.Any(c => c.Path == path);
        }

        public void SetFileId(int movieId, int fileId)
        {
            SetFields(new Movie { Id = movieId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public PagingSpec<Movie> MoviesWithoutFile(PagingSpec<Movie> pagingSpec)
        {
            var currentTime = DateTime.UtcNow;

            pagingSpec.TotalRecords = GetMissingMovieQuery(pagingSpec, currentTime).GetRowCount();
            pagingSpec.Records = GetMissingMovieQuery(pagingSpec, currentTime).ToList();

            return pagingSpec;
        }

        private SortBuilder<Movie> GetMissingMovieQuery(PagingSpec<Movie> pagingSpec, DateTime currentTime)
        {
            return Query.Where(pagingSpec.FilterExpression)
                        .AndWhere(e => e.MovieFileId == 0)
                        .AndWhere(BuildAirDateUtcCutoffWhereClause(currentTime))
                        .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                        .Skip(pagingSpec.PagingOffset())
                        .Take(pagingSpec.PageSize);
        }

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            return String.Format("WHERE datetime(strftime('%s', [t0].[ReleaseDate]) + [t1].[RunTime] * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

    }
}
