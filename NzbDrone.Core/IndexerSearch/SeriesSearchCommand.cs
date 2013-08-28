using System;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchCommand : ICommand
    {
        public String CommandId { get; set; }
        public int SeriesId { get; set; }

        public SeriesSearchCommand()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }
}