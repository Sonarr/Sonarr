using FluentValidation.Results;
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

        public DownloadStationProxy()
        {
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

            var response = ProcessRequest<IEnumerable<DownloadStationTask>>(SynologyApi.DownloadStationTask, arguments, settings);
            var items = response.Data.Select(t => t.ToDownloadClientItem());

            return items;
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

            ProcessRequest<object>(SynologyApi.DownloadStationTask, arguments, settings);
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
                throw new DownloadClientAuthenticationException(response.Error.GetMessage(SynologyApi.Auth));
            }
        }

        private DownloadStationResponse<T> ProcessRequest<T>(SynologyApi api, Dictionary<string, string> arguments, DownloadStationSettings settings)
        {
            var client = BuildClient(settings);
            var response = ProcessRequest<T>(client, api, arguments);

            if (!response.Success)
            {
                if (response.Error.SessionError)
                {
                    Login(client, settings);

                    response = ProcessRequest<T>(client, api, arguments);

                    if (response.Success)
                    {
                        return response;
                    }
                }

                throw new DownloadClientException(response.Error.GetMessage(api));
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
