using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DataAugmentation.Xem.Model;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.DataAugmentation.Xem
{
    public interface IXemProxy
    {
        List<int> GetXemSeriesIds();
        List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id);
    }

    public class XemProxy : IXemProxy
    {
        private readonly Logger _logger;

        private const string XEM_BASE_URL = "http://thexem.de/map/";

        private static readonly string[] IgnoredErrors = { "no single connection", "no show with the tvdb_id" };


        public XemProxy(Logger logger)
        {
            _logger = logger;
        }


        private static RestRequest BuildRequest(string resource)
        {
            var req = new RestRequest(resource, Method.GET);
            req.AddParameter("origin", "tvdb");
            return req;
        }

        public List<int> GetXemSeriesIds()
        {
            _logger.Trace("Fetching Series IDs from");

            var restClient = new RestClient(XEM_BASE_URL);

            var request = BuildRequest("havemap");

            var response = restClient.ExecuteAndValidate<XemResult<List<int>>>(request);
            CheckForFailureResult(response);

            return response.Data.ToList();
        }

        public List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id)
        {
            _logger.Trace("Fetching Mappings for: {0}", id);
            var url = String.Format("{0}all?id={1}&origin=tvdb", XEM_BASE_URL, id);


            var restClient = new RestClient(XEM_BASE_URL);

            var request = BuildRequest("all");
            request.AddParameter("id", id);

            var response = restClient.ExecuteAndValidate<XemResult<List<XemSceneTvdbMapping>>>(request);
            CheckForFailureResult(response);

            return response.Data.Where(c => c.Scene != null).ToList();
        }

        private static void CheckForFailureResult<T>(XemResult<T> response)
        {
            if (response.Result.Equals("failure", StringComparison.InvariantCultureIgnoreCase) &&
                !IgnoredErrors.Any(knowError => response.Message.Contains(knowError)))
            {
                throw new Exception("Error response received from Xem: " + response.Message);
            }
        }


    }
}
