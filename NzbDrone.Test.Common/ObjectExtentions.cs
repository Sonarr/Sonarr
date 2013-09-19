using NzbDrone.Common.Serializer;

namespace NzbDrone.Test.Common
{
    public static class ObjectExtentions
    {
        public static T JsonClone<T>(this T source) where T : new()
        {
            var json = source.ToJson();
            return Json.Deserialize<T>(json);
        }
    }
}