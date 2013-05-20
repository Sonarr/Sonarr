using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Notifications
{
    public interface INotifcationSettings
    {
        bool IsValid { get; }
    }
}
