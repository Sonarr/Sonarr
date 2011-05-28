using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Model
{
    public class Quality : IComparable<Quality>
    {
        public QualityTypes QualityType { get; set; }

        public Boolean Proper { get; set; }

        public Quality() { }

        public Quality(QualityTypes quality, Boolean proper)
        {
            QualityType = quality;
            Proper = proper;
        }

        public int CompareTo(Quality other)
        {
            if (other.QualityType > this.QualityType)
                return -1;

            if (other.QualityType < this.QualityType)
                return 1;

            if (other.QualityType == this.QualityType && other.Proper == this.Proper)
                return 0;

            if (this.Proper && !other.Proper)
                return 1;

            if (!this.Proper && other.Proper)
                return -1;

            return 0;
        }

        public static bool operator !=(Quality x, Quality y)
        {
            return !(x == y);
        }

        public static bool operator ==(Quality x, Quality y)
        {
            var xObj = (Object)x;
            var yObj = (object)y;

            if (xObj == null || yObj == null)
            {
                return xObj == yObj;
            }

            return x.CompareTo(y) == 0;
        }

        public static bool operator >(Quality x, Quality y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(Quality x, Quality y)
        {
            return x.CompareTo(y) < 1;
        }

        public static bool operator <=(Quality x, Quality y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >=(Quality x, Quality y)
        {
            return x.CompareTo(y) >= 0;
        }

        public override string ToString()
        {
            string result = QualityType.ToString();
            if (Proper)
            {
                result += " [proper]";
            }

            return result;
        }
    }
}
