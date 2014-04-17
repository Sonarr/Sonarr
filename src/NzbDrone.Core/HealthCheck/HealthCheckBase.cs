using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.HealthCheck
{
    public abstract class HealthCheckBase : IProvideHealthCheck
    {
        public abstract HealthCheck Check();

        public virtual bool CheckOnStartup
        {
            get
            {
                return true;
            }
        }

        public virtual bool CheckOnConfigChange
        {
            get
            {
                return true;
            }
        }

        public virtual bool CheckOnSchedule
        {
            get
            {
                return true;
            }
        }
    }
}
