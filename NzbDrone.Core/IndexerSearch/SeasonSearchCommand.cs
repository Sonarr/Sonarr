using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeasonSearchCommand : ICommand
    {
        public String CommandId { get; private set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }

        public SeasonSearchCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}