using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public static class NewznabPreProcessor
    {
        public static void Process(string source, string url)
        {
            var xdoc = XDocument.Parse(source);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null) return;

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199) throw new ApiKeyException("Invalid API key");

            if (!url.Contains("apikey=") && errorMessage == "Missing parameter")
            {
                throw new ApiKeyException("Indexer requires an API key");
            }

            throw new NewznabException("Newznab error detected: {0}", errorMessage);
        }
    }
}
