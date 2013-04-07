using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Newtonsoft.Json;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Nzbx;

namespace NzbDrone.Core.Indexers.Nzbx
{
    public class NzbxParser : IParseFeed
    {
        private readonly Logger _logger;
        private readonly JsonSerializer _serializer;

        public NzbxParser(Logger logger)
        {
            _logger = logger;
            _serializer = new JsonSerializer();
        }

        public IEnumerable<EpisodeParseResult> Process(Stream source)
        {
            var result = new List<EpisodeParseResult>();
            var jsonReader = new JsonTextReader(new StreamReader(source));
            var feed = _serializer.Deserialize<List<NzbxRecentItem>>(jsonReader);

            foreach (var item in feed)
            {
                try
                {
                    var episodeParseResult = Parser.ParseTitle(item.Name);
                    if (episodeParseResult != null)
                    {
                        episodeParseResult.Age = DateTime.Now.Date.Subtract(item.PostDate).Days;
                        episodeParseResult.OriginalString = item.Name;
                        episodeParseResult.SceneSource = true;
                        episodeParseResult.NzbUrl = String.Format("http://nzbx.co/nzb?{0}*|*{1}", item.Guid, item.Name);
                        episodeParseResult.NzbInfoUrl = String.Format("http://nzbx.co/d?{0}", item.Guid);
                        episodeParseResult.Size = item.Size;

                        result.Add(episodeParseResult);
                    }
                }
                catch (Exception itemEx)
                {
                    itemEx.Data.Add("Item", item.Name);
                    _logger.ErrorException("An error occurred while processing feed item", itemEx);
                }
            }

            return result;
        }
    }
}