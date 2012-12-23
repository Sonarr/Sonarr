using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NzbDrone.Core.Helpers
{
    public static class XElementHelper
    {
        public static T ConvertTo<T>(this XElement element)
        {
            if (element == null)
                return default(T);

            if (String.IsNullOrEmpty(element.Value))
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                return (T)converter.ConvertFromString(element.Value);
            }

            catch
            {
                return default(T);
            }
        }

        public static DayOfWeek? ConvertToDayOfWeek(this XElement element)
        {
            if (element == null)
                return null;

            if (String.IsNullOrWhiteSpace(element.Value))
                return null;

            try
            {
                return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), element.Value);
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
