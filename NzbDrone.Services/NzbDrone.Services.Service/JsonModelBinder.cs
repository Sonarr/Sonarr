using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Newtonsoft.Json;

namespace NzbDrone.Services.Service
{
    public class JsonModelBinder : DefaultModelBinder
    {
        private static readonly JsonSerializer serializer = new JsonSerializer();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            try
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
            catch (Exception e)
            {
                logger.FatalException("Error while binding model.", e);
                throw;
            }
        }

        private static bool IsJsonRequest(HttpRequestBase request)
        {
            return request.ContentType.ToLower().Contains("application/json");
        }
    }
}