using System;
using System.Linq.Expressions;

namespace NzbDrone.Common.EnsureThat
{
    public static class Ensure
    {
        public static Param<T> That<T>(T value, string name = Param.DefaultName)
        {
            return new Param<T>(name, value);
        }

        public static Param<T> That<T>(T value, Expression<Func<T>> expression)
        {
            return new Param<T>(expression.GetPath(), value);
        }

        public static TypeParam ThatTypeFor<T>(T value, string name = Param.DefaultName)
        {
            return new TypeParam(name, value.GetType());
        }
    }
}
