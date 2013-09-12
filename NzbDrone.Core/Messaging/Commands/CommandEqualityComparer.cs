using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandEqualityComparer : IEqualityComparer<Command>
    {
        public static readonly CommandEqualityComparer Instance = new CommandEqualityComparer();

        private CommandEqualityComparer()
        {

        }

        public bool Equals(Command x, Command y)
        {
            var xProperties = x.GetType().GetProperties();
            var yProperties = y.GetType().GetProperties();

            foreach (var xProperty in xProperties)
            {
                if (xProperty.Name == "Id")
                {
                    continue;
                }

                var yProperty = yProperties.SingleOrDefault(p => p.Name == xProperty.Name);

                if (yProperty == null)
                {
                    continue;
                }

                var xValue = xProperty.GetValue(x, null);
                var yValue = yProperty.GetValue(y, null);

                if (xValue == null && yValue == null)
                {
                    return true;
                }

                if (xValue == null || yValue == null)
                {
                    return false;
                }

                if (!xValue.Equals(yValue))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(Command obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
