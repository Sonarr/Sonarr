using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MediaBrowserProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            var path = "/Notifications/Admin";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";

            request.SetContent(new
                           {
                               Name = title,
                               Description = message,
                               ImageUrl = "https://raw.github.com/NzbDrone/NzbDrone/develop/Logo/64.png"
                           }.ToJson());

            ProcessRequest(request, settings);
        }

        public List<string> GetPaths(MediaBrowserSettings settings, Series series)
        {
            var path = "/Items";
            var url = GetUrl(settings);

            // NameStartsWith uses the sort title, which is not the series title
            var request = new HttpRequestBuilder(url)
                .Resource(path)
                .AddQueryParam("recursive", "true")
                .AddQueryParam("includeItemTypes", "Series")
                .AddQueryParam("fields", "Path,ProviderIds")
                .AddQueryParam("years", series.Year)
                .Build();

            try
            {
                var paths = ProcessGetRequest<MediaBrowserItems>(request, settings).Items.GroupBy(item =>
                {
                    var accumulator = 0;

                    if (item is { ProviderIds.Tvdb: int tvdbid } && tvdbid != 0 && tvdbid == series.TvdbId)
                    {
                        accumulator |= 1 << 4;
                    }

                    if (item is { ProviderIds.Imdb: string imdbid } && imdbid == series.ImdbId)
                    {
                        accumulator |= 1 << 3;
                    }

                    if (item is { ProviderIds.TvMaze: int tvmazeid } && tvmazeid != 0 && tvmazeid == series.TvMazeId)
                    {
                        accumulator |= 1 << 2;
                    }

                    if (item is { ProviderIds.TvRage: int tvrageid } && tvrageid != 0 && tvrageid == series.TvRageId)
                    {
                        accumulator |= 1 << 1;
                    }

                    if (item is { Name: var name } && name == series.Title)
                    {
                        accumulator |= 1 << 0;
                    }

                    _logger.Trace($"{item.Path} {accumulator} {item.ProviderIds.TvRage} {series.TvRageId}");

                    return -accumulator;
                }, item => item.Path).OrderBy(group => group.Key).First();

                if (paths.Key == 0)
                {
                    throw new MediaBrowserException("Could not find series by name");
                }

                _logger.Trace("Found series by name: {0}", string.Join(" ", paths));

                return paths.ToList();
            }
            catch (InvalidOperationException ex)
            {
                throw new MediaBrowserException("Could not find series by name", ex);
            }
        }

        public void Update(MediaBrowserSettings settings, string seriesPath, string updateType)
        {
            var path = "/Library/Media/Updated";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";

            request.SetContent(new
            {
                Updates = new[]
                {
                    new
                    {
                        Path = seriesPath,
                        UpdateType = updateType
                    }
                }
            }.ToJson());

            ProcessRequest(request, settings);
        }

        private T ProcessGetRequest<T>(HttpRequest request, MediaBrowserSettings settings)
            where T : new()
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Get<T>(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Resource;
        }

        private string ProcessRequest(HttpRequest request, MediaBrowserSettings settings)
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Post(request);
            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private string GetUrl(MediaBrowserSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            return $@"{scheme}://{settings.Address}";
        }

        private HttpRequest BuildRequest(string path, MediaBrowserSettings settings)
        {
            var url = GetUrl(settings);

            return new HttpRequestBuilder(url).Resource(path).Build();
        }

        private void CheckForError(HttpResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            // TODO: actually check for the error
        }
    }
}
