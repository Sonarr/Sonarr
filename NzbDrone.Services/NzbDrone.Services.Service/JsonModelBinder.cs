using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace NzbDrone.Services.Service
{
    public class JsonModelBinder : DefaultModelBinder
    {
        private static readonly JsonSerializer serializer = new JsonSerializer();

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            if (!IsJsonRequest(request))
            {
                return base.BindModel(controllerContext, bindingContext);
            }

            object deserializedObject;
            using (var stream = request.InputStream)
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream))
                {
                    deserializedObject = serializer.Deserialize(reader, bindingContext.ModelMetadata.ModelType);
                }
            }

            return deserializedObject;
        }

        private static bool IsJsonRequest(HttpRequestBase request)
        {
            return request.ContentType.ToLower().Contains("application/json");
        }
    }
}