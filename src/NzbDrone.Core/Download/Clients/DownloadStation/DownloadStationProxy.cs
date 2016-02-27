using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Rest;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public interface IDownloadStationProxy
    {
        void Test(List<ValidationFailure> failures, DownloadStationSettings settings);
        IEnumerable<DownloadClientItem> GetItems(DownloadStationSettings settings);
        DownloadClientStatus GetStatus(DownloadStationSettings settings);
        void RemoveItem(string downloadId, bool deleteData, DownloadStationSettings settings);
        string AddFromUrl(RemoteEpisode remoteEpisode, string url, DownloadStationSettings settings);
        string AddFromFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent, DownloadStationSettings settings);
    }

    public class DownloadStationProxy : IDownloadStationProxy
    {
        private readonly Logger _logger;
        private static readonly Dictionary<SynologyApi, string> Resources;
        private readonly CookieContainer _cookieContainer;

        static DownloadStationProxy()
        {
            Resources = new Dictionary<SynologyApi, string>
            {
                {SynologyApi.Auth, "/auth.cgi"},
                {SynologyApi.DownloadStationInfo, "/DownloadStation/info.cgi"},
                {SynologyApi.DownloadStationTask, "/DownloadStation/task.cgi"}
            };
        }

        public DownloadStationProxy(Logger logger)
        {
            _logger = logger;
            _cookieContainer = new CookieContainer();
        }

        public string AddFromFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public string AddFromUrl(RemoteEpisode remoteEpisode, string url, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DownloadClientItem> GetItems(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "1"},
                {"method", "list"},
                {"additional", "detail,transfer"}
            };

            try
            {
                var response = ProcessRequest<IEnumerable<DownloadStationTask>>(SynologyApi.DownloadStationTask, arguments, settings);
                var items = response.Data.Select(t => t.ToDownloadClientItem());

                return items;
            }
            catch (DownloadClientException)
            {
                _logger.Debug("Failed to get items from Download Station");

                throw;
            }
        }

        public DownloadClientStatus GetStatus(DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(string downloadId, bool deleteData, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "1"},
                {"method", "delete"},
                {"id", downloadId},
                {"force_complete", deleteData.ToString()}
            };

            try
            {
                ProcessRequest<object>(SynologyApi.DownloadStationTask, arguments, settings);
            }
            catch (DownloadClientException)
            {
                _logger.Debug(string.Format("Failed to remove item {0} from Download Station", downloadId));

                throw;
            }

            _logger.Trace(string.Format("Item {0} removed from Download Station", downloadId));
        }

        public void Test(List<ValidationFailure> failures, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        private void Login(IRestClient client, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.API.Auth"},
                {"version", "1"},
                {"method", "login"},
                {"account", settings.Username},
                {"passwd", settings.Password},
                {"session", "DownloadStation"},
            };

            var response = ProcessRequest<object>(client, SynologyApi.Auth, arguments);

            if (!response.Success)
            {
                var message = response.Error.GetMessage(SynologyApi.Auth);
                _logger.Debug(string.Format("Failed to log in to Download Station: {0} - {1}", response.Error.Code, message));

                throw new DownloadClientAuthenticationException(message);
            }

            _logger.Trace("Logged in to Download Station");
        }

        private DownloadStationResponse<T> ProcessRequest<T>(SynologyApi api, Dictionary<string, string> arguments, DownloadStationSettings settings)
        {
            var client = BuildClient(settings);
            var response = ProcessRequest<T>(client, api, arguments);

            if (!response.Success)
            {
                if (response.Error.SessionError)
                {
                    _logger.Debug(string.Format("Download Station session error: {0} - {1}", response.Error.Code, response.Error.GetMessage(api)));

                    Login(client, settings);

                    response = ProcessRequest<T>(client, api, arguments);

                    if (response.Success)
                    {
                        return response;
                    }
                }

                var message = response.Error.GetMessage(api);
                _logger.Debug(string.Format("Download Station request failed: {0} - {1}", response.Error.Code, message));

                throw new DownloadClientException(message);
            }

            return response;
        }

        private DownloadStationResponse<T> ProcessRequest<T>(IRestClient client, SynologyApi api, Dictionary<string, string> arguments)
        {
            var request = new RestRequest(Method.GET)
            {
                Resource = Resources[api]
            };

            foreach (var arg in arguments)
            {
                request.AddParameter(arg.Key, arg.Value);
            }

            var response = client.Execute(request);
            var result = response.Read<DownloadStationResponse<T>>(client);

            return result;
        }

        private IRestClient BuildClient(DownloadStationSettings settings)
        {
            var url = string.Format("http://{0}:{1}/webapi", settings.Host, settings.Port);
            var client = RestClientFactory.BuildClient(url);

            client.CookieContainer = _cookieContainer;

            return client;
        }
    }
}
