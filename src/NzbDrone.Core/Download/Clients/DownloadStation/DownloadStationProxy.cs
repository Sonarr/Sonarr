using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
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
        private static readonly Dictionary<SynologyApi, string> Resources;
        private readonly Logger _logger;
        private readonly RemotePathMappingService _remotePathMappingService;
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

        public DownloadStationProxy(RemotePathMappingService remotePathMappingService, Logger logger)
        {
            _remotePathMappingService = remotePathMappingService;
            _logger = logger;
            _cookieContainer = new CookieContainer();
        }

        public string AddFromFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent, DownloadStationSettings settings)
        {
            throw new NotImplementedException();
        }

        public string AddFromUrl(RemoteEpisode remoteEpisode, string url, DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "3"},
                {"method", "create"},
                {"uri", url}
            };

            try
            {
                var response = ProcessRequest<object>(SynologyApi.DownloadStationTask, arguments, settings);

                try
                {
                    return GetTaskId(url, settings);
                }
                catch (DownloadClientException)
                {
                    _logger.Debug(string.Format("Task with URL {0} added to Download Station but failed to get task ID", url));

                    throw;
                }
            }
            catch (DownloadClientException)
            {
                _logger.Debug(string.Format("Failed to add task with URL {0} to Download Station", url));

                throw;
            }
        }

        public IEnumerable<DownloadClientItem> GetItems(DownloadStationSettings settings)
        {
            try
            {
                var tasks = GetTasks(settings);
                var items = tasks.Select(t => t.ToDownloadClientItem(_remotePathMappingService, settings));

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
            return new DownloadClientStatus
            {
                IsLocalhost = settings.Host == "127.0.0.1" || settings.Host == "localhost",
                OutputRootFolders = new List<OsPath>()
            };
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
            try
            {
                Login(BuildClient(settings), settings);

                var version = GetVersion(settings);

                if (version < 3)
                {
                    failures.Add(new ValidationFailure(string.Empty, string.Format("Incompatible API version: 3 and later supported")));
                }
            }
            catch (DownloadClientException e)
            {
                failures.Add(new ValidationFailure(string.Empty, e.Message));
            }
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

        private int GetVersion(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "1"},
                {"method", "getinfo"}
            };

            var response = ProcessRequest<DownloadStationInfo>(SynologyApi.DownloadStationInfo, arguments, settings);

            return int.Parse(response.Data.Version.Split('.').First());
        }

        private IEnumerable<DownloadStationTask> GetTasks(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "1"},
                {"method", "list"},
                {"additional", "detail,transfer"}
            };

            var response = ProcessRequest<DownloadStationTaskCollection>(SynologyApi.DownloadStationTask, arguments, settings);

            return response.Data;
        }

        private string GetTaskId(string uri, DownloadStationSettings settings)
        {
            var tasks = GetTasks(settings).Where(t => t.Additional.Detail.Uri == uri);

            if (tasks.Any())
            {
                try
                {
                    return tasks.Single().Id;
                }
                catch (InvalidOperationException)
                {
                    var ids = string.Join(",", tasks.Select(t => t.Id));
                    _logger.Debug(string.Format("Multiple tasks with URI {0} in Download Station: {1}", uri, ids));
                }
            }
            else
            {
                _logger.Debug(string.Format("No such task with URI {0} in Download Station", uri));
            }

            throw new DownloadClientException("Failed to get task ID from Download Station");
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
