using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.ReferenceData
{
    public interface ISceneMappingProxy
    {
        List<SceneMapping> Fetch();
    }

    public class SceneMappingProxy : ISceneMappingProxy
    {
        private readonly HttpProvider _httpProvider;
        private readonly IConfigService _configService;

        public SceneMappingProxy(HttpProvider httpProvider, IConfigService configService)
        {
            _httpProvider = httpProvider;
            _configService = configService;
        }

        public List<SceneMapping> Fetch()
        {
            var mappingsJson = _httpProvider.DownloadString(_configService.ServiceRootUrl + "/SceneMapping/Active");
            return JsonConvert.DeserializeObject<List<SceneMapping>>(mappingsJson);
        }


        /*        public virtual bool SubmitMapping(int id, string postTitle)
                {
                    Logger.Trace("Parsing example post");
                    var episodeParseResult = Parser.ParseTitle(postTitle);
                    var cleanTitle = episodeParseResult.CleanTitle;
                    var title = episodeParseResult.SeriesTitle.Replace('.', ' ');
                    Logger.Trace("Example post parsed. CleanTitle: {0}, Title: {1}", cleanTitle, title);

                    var newMapping = String.Format("/SceneMapping/AddPending?cleanTitle={0}&id={1}&title={2}", cleanTitle, id, title);
                    var response = _httpProvider.DownloadString(_configService.ServiceRootUrl + newMapping);

                    if (JsonConvert.DeserializeObject<String>(response).Equals("Ok"))
                        return true;

                    return false;
                }*/
    }
}