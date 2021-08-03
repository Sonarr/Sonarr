using System.Collections;
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
            if (x.GetType() != y.GetType())
            {
                return false;
            }

            var xProperties = x.GetType().GetProperties();
            var yProperties = y.GetType().GetProperties();

            foreach (var xProperty in xProperties)
            {
                if (xProperty.Name == "Id")
                {
                    continue;
                }

                if (xProperty.DeclaringType == typeof(Command))
                {
                    continue;
                }

                var yProperty = yProperties.Single(p => p.Name == xProperty.Name);

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

                if (typeof(IEnumerable).IsAssignableFrom(xProperty.PropertyType))
                {
                    var xValueCollection = ((IEnumerable)xValue).Cast<object>();
                    var yValueCollection = ((IEnumerable)yValue).Cast<object>();

                    var xNotY = xValueCollection.Except(yValueCollection);
                    var yNotX = yValueCollection.Except(xValueCollection);

                    if (xNotY.Any() || yNotX.Any())
                    {
                        return false;
                    }
                }
                else if (!xValue.Equals(yValue))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(Command obj)
        {
            return obj.GetHashCode();
        }
    }
}
