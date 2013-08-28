using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchCommand : ICommand
    {
        public String CommandId { get; set; }
        public int EpisodeId { get; set; }
        
        public EpisodeSearchCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}