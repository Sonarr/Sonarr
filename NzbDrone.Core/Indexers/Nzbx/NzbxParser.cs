using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Newtonsoft.Json;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Nzbx
{
    public class NzbxParser : IParseFeed
    {
        private readonly Logger _logger;
        private readonly JsonSerializer _serializer;

        public NzbxParser()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _serializer = new JsonSerializer();
        }

        public IEnumerable<ReportInfo> Process(Stream source)
        {
            var result = new List<ReportInfo>();
            var jsonReader = new JsonTextReader(new StreamReader(source));
            var feed = _serializer.Deserialize<List<NzbxRecentItem>>(jsonReader);

            foreach (var item in feed.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                try
                {
                    var reportInfo = new ReportInfo();
                    reportInfo.Age = DateTime.Now.Date.Subtract(item.PostDate).Days;
                    reportInfo.Title = item.Name;
                    reportInfo.NzbUrl = String.Format("http://nzbx.co/nzb?{0}*|*{1}", item.Guid, item.Name);
                    reportInfo.NzbInfoUrl = String.Format("http://nzbx.co/d?{0}", item.Guid);
                    reportInfo.Size = item.Size;

                    result.Add(reportInfo);
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