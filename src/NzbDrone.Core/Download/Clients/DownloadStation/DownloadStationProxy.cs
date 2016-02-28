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
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Task"},
                {"version", "1"},
                {"method", "create"},
            };

            var request = BuildRequest(Method.POST, SynologyApi.DownloadStationTask, arguments);
            request.AddFile("file", fileContent, filename);

            return AddTask(request, filename, settings);
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

            var request = BuildRequest(Method.POST, SynologyApi.DownloadStationTask, arguments);

            return AddTask(request, url, settings);
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
            var arguments = new Dictionary<string, string>
            {
                {"api", "SYNO.DownloadStation.Info"},
                {"version", "1"},
                {"method", "getconfig"}
            };

            try
            {
                var response = ProcessGetRequest<Dictionary<string, string>>(SynologyApi.DownloadStationInfo, arguments, settings);
                var path = new OsPath(response.Data["default_destination"]);

                return new DownloadClientStatus
                {
                    IsLocalhost = settings.Host == "127.0.0.1" || settings.Host == "localhost",
                    OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(settings.Host, path) }
                };
            }
            catch (DownloadClientException)
            {
                _logger.Debug("Failed to get config from Download Station");

                throw;
            }
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
                ProcessGetRequest<object>(SynologyApi.DownloadStationTask, arguments, settings);
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
                var client = BuildClient(settings);

                Login(client, settings);

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
            catch (WebException e)
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

            var request = BuildRequest(Method.GET, SynologyApi.Auth, arguments);
            var response = ProcessRequest<object>(client, request);

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

            var response = ProcessGetRequest<Dictionary<string, string>>(SynologyApi.DownloadStationInfo, arguments, settings);

            return int.Parse(response.Data["version_string"].Split('.').First());
        }

        private string AddTask(IRestRequest request, string uri, DownloadStationSettings settings)
        {
            try
            {
                var response = ProcessRequest<object>(SynologyApi.DownloadStationTask, request, settings);
            }
            catch (DownloadClientException)
            {
                _logger.Debug(string.Format("Failed to add task {0} to Download Station", uri));

                throw;
            }

            try
            {
                return GetTaskId(uri, settings);
            }
            catch (DownloadClientException)
            {
                _logger.Debug(string.Format("Task {0} added to Download Station but failed to get task ID", uri));

                throw;
            }
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

            var response = ProcessGetRequest<DownloadStationTaskCollection>(SynologyApi.DownloadStationTask, arguments, settings);

            return response.Data;
        }

        private string GetTaskId(string uri, DownloadStationSettings settings)
        {
            var tasks = GetTasks(settings).Where(t => t.Additional.Detail["uri"] == uri);

            if (tasks.Any())
            {
                try
                {
                    return tasks.Single().Id;
                }
                catch (InvalidOperationException)
                {
                    var ids = string.Join(",", tasks.Select(t => t.Id));
                    _logger.Debug(string.Format("Multiple {0} tasks in Download Station: {1}", uri, ids));
                }
            }
            else
            {
                _logger.Debug(string.Format("No such task {0} in Download Station", uri));
            }

            throw new DownloadClientException("Failed to get task ID from Download Station");
        }

        private DownloadStationResponse<T> ProcessGetRequest<T>(SynologyApi api, Dictionary<string, string> arguments, DownloadStationSettings settings)
        {
            var request = BuildRequest(Method.GET, api, arguments);

            return ProcessRequest<T>(SynologyApi.DownloadStationTask, request, settings);
        }

        private DownloadStationResponse<T> ProcessRequest<T>(SynologyApi api, IRestRequest request, DownloadStationSettings settings)
        {
            var client = BuildClient(settings);
            var response = ProcessRequest<T>(client, request);

            if (!response.Success)
            {
                if (response.Error.SessionError)
                {
                    _logger.Debug(string.Format("Download Station session error: {0} - {1}", response.Error.Code, response.Error.GetMessage(api)));

                    Login(client, settings);

                    response = ProcessRequest<T>(client, request);

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

        private DownloadStationResponse<T> ProcessRequest<T>(IRestClient client, IRestRequest request)
        {
            var response = client.Execute(request);
            var result = response.Read<DownloadStationResponse<T>>(client);

            return result;
        }

        private IRestRequest BuildRequest(Method method, SynologyApi api, Dictionary<string, string> arguments)
        {
            var request = new RestRequest(method)
            {
                Resource = Resources[api]
            };

            foreach (var arg in arguments)
            {
                request.AddParameter(arg.Key, arg.Value);
            }

            return request;
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
