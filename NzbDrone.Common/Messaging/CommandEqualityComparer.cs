using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Messaging
{
    public class CommandEqualityComparer : IEqualityComparer<ICommand>
    {
        public bool Equals(ICommand x, ICommand y)
        {
            var xProperties = x.GetType().GetProperties();
            var yProperties = y.GetType().GetProperties();

            foreach (var xProperty in xProperties)
            {
                if (xProperty.Name == "CommandId")
                {
                    continue;
                }

                var yProperty = yProperties.SingleOrDefault(p => p.Name == xProperty.Name);

                if (yProperty == null)
                {
                    continue;
                }

                if (!xProperty.GetValue(x, null).Equals(yProperty.GetValue(y, null)))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ICommand obj)
        {
            return obj.CommandId.GetHashCode();
        }
    }
}
