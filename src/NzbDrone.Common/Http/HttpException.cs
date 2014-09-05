using System;
using RestSharp;

namespace NzbDrone.Common.Http
{
    public class HttpException : Exception
    {
        public IRestResponse Response { get; private set; }

        public HttpException(IRestResponse response, IRestClient restClient)
            : base(string.Format("REST request failed: [{0}] [{1}] at [{2}]", (int)response.StatusCode, response.Request.Method, restClient.BuildUri(response.Request)))
        {
            Response = response;
        }

        public override string ToString()
        {
            if (Response != null)
            {
                return base.ToString() + Environment.NewLine + Response.Content;
            }

            return base.ToString();
        }
    }
}