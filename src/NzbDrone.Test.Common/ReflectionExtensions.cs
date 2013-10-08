namespace NzbDrone.Test.Common
{
    public static class ReflectionExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T)obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }
    }
}
