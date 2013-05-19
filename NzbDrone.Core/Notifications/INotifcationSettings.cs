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

    public class NullSetting : INotifcationSettings
    {
        public static NullSetting Instance = new NullSetting();

        private NullSetting()
        {

        }

        public bool IsValid
        {
            get
            {
                return true;
            }
        }
    }
}
