using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Movies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public class TmdbProxy : ISearchForNewMovie, IProvideMoviesInfo
    {
        private readonly Logger _logger;
        private readonly HttpClient _httpClient;
        private static readonly Regex CollapseSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex RemoveYearRegex = new Regex(@"(?:\(\d+\))", RegexOptions.Compiled);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"(?:\*|\(|\)|'|!)", RegexOptions.Compiled);
        private readonly HttpRequestBuilder _requestBuilder;
        private static readonly String API_KEY = "8d9296f6c717e906d76ce419ede18d1b";
        private static readonly int MAX_RETRIES = 3;
        private static readonly int GRACETIME = 500;

        public TmdbProxy(Logger logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _requestBuilder = new HttpRequestBuilder("http://api.themoviedb.org/3/");
        }


        private static string GetPosterThumbnailUrl(string posterUrl)
        {
            if (posterUrl.Contains("poster-small.jpg")) return posterUrl;

            var extension = Path.GetExtension(posterUrl);
            var withoutExtension = posterUrl.Substring(0, posterUrl.Length - extension.Length);
            return withoutExtension + "-300" + extension;
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

        public IEnumerable<Movies.Movie> SearchForNewMovie(string title)
        {

            var httpRequest = _requestBuilder.Build("search/movie");
            httpRequest.AddQueryParam("api_key", API_KEY);
            httpRequest.AddQueryParam("query", GetSearchTerm(title));

            var tries = 0;
            var done = false;
            MovieSearchResource resource = new MovieSearchResource();
            while (tries++ < MAX_RETRIES)
            {
                try
                {
                    var httpResponse = _httpClient.Get<MovieSearchResource>(httpRequest);
                    resource = httpResponse.Resource;
                    done = true;
                    break;
                }
                catch (TooManyRequestsException ex)
                {

                    var waitTime = Int32.Parse(ex.Response.Headers["Retry-After"].ToString())*1000 + GRACETIME;
                    _logger.Warn("Too many request for tmdb, waiting {0}ms", waitTime);
                    Thread.Sleep(waitTime);
                }
            }

            if (!done)
            {
                throw new Exception("Error fetching data from tmdb");
            }

            foreach (var movie in resource.results.Take(10))
            {
                yield return MapMovie(movie);
            }
        }

        private Movie MapMovie(MovieResource movie)
        {
            var newMovie = new Movies.Movie();
            var httpRequest = _requestBuilder.Build("movie/"+movie.id);
            httpRequest.AddQueryParam("api_key", API_KEY);

            var tries = 0;
            var done = false;
            while (tries++ < MAX_RETRIES)
            {
                try
                {
                    var httpResponse = _httpClient.Get<MovieResource>(httpRequest);
                    if (httpResponse.HasHttpError)
                    {
                        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new MovieNotFoundException(movie.id);
                        }
                        else
                        {
                            throw new NzbDrone.Common.Http.HttpException(httpRequest, httpResponse);
                        }
                    }
                    movie = httpResponse.Resource;
                    done = true;
                    break;
                }
                catch (TooManyRequestsException ex)
                {
                    var waitTime = Int32.Parse(ex.Response.Headers["Retry-After"].ToString())*1000 + GRACETIME;
                    _logger.Warn("Too many request for tmdb, waiting {0}ms", waitTime);
                    
                    Thread.Sleep(waitTime);
                }
            }

            if (!done)
            {
                throw new Exception("Error fetching data from tmdb");
            }

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
            } catch (Exception e)
            {
            }
            newMovie.Overview = movie.overview;
            newMovie.Runtime = movie.runtime;
            //newMovie.TitleSlug = movie.url.ToLower().Replace("http://trackt.tv/movie/", "");

            //newMovie.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Banner, Url = movie.images.banner });
            if (movie.poster_path != null)
                newMovie.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Poster, 
                                                                Url = "http://image.tmdb.org/t/p/w154" + movie.poster_path, 
                                                                CoverOrigin = MediaCoverOrigin.Movie});
            if (movie.backdrop_path != null)
                newMovie.Images.Add(new MediaCover.MediaCover { CoverType = MediaCoverTypes.Fanart,
                                                                Url = "http://image.tmdb.org/t/p/w780" + movie.backdrop_path,
                                                                CoverOrigin = MediaCoverOrigin.Movie });
            return newMovie;
        }

        public Movie GetMovieInfo(int tmdbMovieId)
        {
            return MapMovie(new MovieResource { id = tmdbMovieId });
        }
    }
}