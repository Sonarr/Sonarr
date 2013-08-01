using Newtonsoft.Json;

namespace NzbDrone.Test.Common
{
    public static class ObjectExtentions
    {
        public static T JsonClone<T>(this T source)
        {
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}