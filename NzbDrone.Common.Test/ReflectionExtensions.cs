using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Test
{
    public static class ReflectionExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T)obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }
    }
}
