using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Update.Commands
{
    public class InstallUpdateCommand : Command
    {
        public UpdatePackage UpdatePackage { get; set; }
    }
}
