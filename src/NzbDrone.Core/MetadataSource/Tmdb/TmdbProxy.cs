using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public class TmdbProxy : ISearchForNewMovie, IProvideMoviesInfo
    {
        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;
        private static readonly Regex CollapseSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex RemoveYearRegex = new Regex(@"(?:\(\d+\))", RegexOptions.Compiled);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"(?:\*|\(|\)|'|!)", RegexOptions.Compiled);
        private readonly HttpRequestBuilder _requestBuilder;
        private static readonly String API_KEY = "8d9296f6c717e906d76ce419ede18d1b";
        private static readonly int MAX_RETRIES = 3;
        private static readonly int GRACETIME = 500;

        public TmdbProxy(Logger logger, IHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _requestBuilder = new HttpRequestBuilder("http://api.themoviedb.org/3/");
        }

        private static string GetSearchTerm(string phrase)
        {
            phrase = phrase.RemoveAccent().ToLower();
            phrase = RemoveYearRegex.Replace(phrase, "");
            phrase = InvalidSearchCharRegex.Replace(phrase, "");
            phrase = CollapseSpaceRegex.Replace(phrase, " ").Trim().ToLower();
            phrase = HttpUtility.UrlEncode(phrase);

            return phrase;
        }

        public IEnumerable<Movie> SearchForNewMovie(string title)
        {

            var lowerTitle = title.ToLowerInvariant();

            if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:"))
            {
                var slug = lowerTitle.Split(':')[1].Trim();

                int tmdbId;

                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !Int32.TryParse(slug, out tmdbId) || tmdbId <= 0)
                {
                    yield break;
                }

                yield return MapMovie(new MovieResource { id = tmdbId });
            }

            if (lowerTitle.StartsWith("imdb:") || lowerTitle.StartsWith("imdbid:"))
            {
                var imdbId = lowerTitle.Split(':')[1].Trim();

                if (imdbId.IsNullOrWhiteSpace() || imdbId.Any(char.IsWhiteSpace))
                {
                    yield break;
                }

                var httpRequest = _requestBuilder.Build("find/" + imdbId);
                httpRequest.AddQueryParam("external_source", "imdb_id");
                httpRequest.AddQueryParam("api_key", API_KEY);
                var httpResponse = GetHttpResponse<MovieSearchResource>(httpRequest);
                var resource = httpResponse.Resource;

                foreach (var movie in resource.movie_results.Take(10))
                {
                    yield return MapMovie(movie);
                }
            }
            else
            {
                var httpRequest = _requestBuilder.Build("search/movie");
                httpRequest.AddQueryParam("query", GetSearchTerm(title));
                httpRequest.AddQueryParam("api_key", API_KEY);
                var httpResponse = GetHttpResponse<MovieSearchResource>(httpRequest);
                var resource = httpResponse.Resource;

                foreach (var movie in resource.results.Take(10))
                {
                    yield return MapMovie(movie);
                }
            }
        }

        private HttpResponse<T> GetHttpResponse<T>(Common.Http.HttpRequest httpRequest) where T : new()
        {
            var tries = 0;
            HttpResponse<T> httpResponse = null;

            while (tries++ < MAX_RETRIES && httpResponse == null)
            {
                try
                {
                    httpResponse = _httpClient.Get<T>(httpRequest);
                }
                catch (TooManyRequestsException ex)
                {

                    var waitTime = Int32.Parse(ex.Response.Headers["Retry-After"].ToString()) * 1000 + GRACETIME;
                    _logger.Warn("Too many request for tmdb, waiting {0}ms", waitTime);
                    Thread.Sleep(waitTime);
                }
            }

            if (httpResponse == null)
            {
                throw new Exception("Error fetching data from tmdb");
            }

            return httpResponse;
        }

        private Movie MapMovie(MovieResource movie)
        {
            var newMovie = new Movie();
            var httpRequest = _requestBuilder.Build("movie/" + movie.id);
            httpRequest.AddQueryParam("api_key", API_KEY);
            httpRequest.SuppressHttpError = true;

            var httpResponse = GetHttpResponse<MovieResource>(httpRequest);
            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(movie.id);
                }
                throw new Common.Http.HttpException(httpRequest, httpResponse);
            }

            movie = httpResponse.Resource;

            newMovie.TmdbId = movie.id;
            newMovie.OriginalTitle = movie.original_title;
            newMovie.ImdbId = movie.imdb_id;
            newMovie.Title = movie.title;
            newMovie.TagLine = movie.tagline;
            DateTime releaseDate;
            DateTime.TryParse(movie.release_date, out releaseDate);
            newMovie.ReleaseDate = releaseDate;
            newMovie.CleanTitle = Parser.Parser.CleanSeriesTitle(movie.title);
            try
            {
                newMovie.Year = DateTime.Parse(movie.release_date).Year;
            }
            catch (Exception)
            {
            }
            newMovie.Overview = movie.overview;
            newMovie.Runtime = movie.runtime;

            if (movie.poster_path != null)
            {
                newMovie.Images.Add(new MediaCover.MediaCover
                {
                    CoverType = MediaCoverTypes.Poster,
                    Url = "http://image.tmdb.org/t/p/w154" + movie.poster_path,
                    CoverOrigin = MediaCoverOrigin.Movie
                });
            }
            if (movie.backdrop_path != null)
            {
                newMovie.Images.Add(new MediaCover.MediaCover
                {
                    CoverType = MediaCoverTypes.Fanart,
                    Url = "http://image.tmdb.org/t/p/w780" + movie.backdrop_path,
                    CoverOrigin = MediaCoverOrigin.Movie
                });
            }
            return newMovie;
        }

        public Movie GetMovieInfo(int tmdbMovieId)
        {
            return MapMovie(new MovieResource { id = tmdbMovieId });
        }
    }
}