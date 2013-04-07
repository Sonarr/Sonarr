using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class DailyEpisodeSearchDefinition : SearchDefinitionBase
    {
        public DateTime Airtime { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1}", SceneTitle, Airtime);
        }
    }
}