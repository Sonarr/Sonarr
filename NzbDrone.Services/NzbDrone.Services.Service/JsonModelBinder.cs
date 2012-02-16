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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var input = "[NULL]";

            try
            {
                var request = controllerContext.HttpContext.Request;

                if (!IsJsonRequest(request))
                {
                    return base.BindModel(controllerContext, bindingContext);
                }

                using (var stream = request.InputStream)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream))
                    {
                        input = reader.ReadToEnd();
                    }
                }

                var deserializedObject = JsonConvert.DeserializeObject(input, bindingContext.ModelMetadata.ModelType);

                return deserializedObject;
            }
            catch (Exception e)
            {
                logger.FatalException("Error deserilizing request. " + input, e);
                throw;
            }
        }

        private static bool IsJsonRequest(HttpRequestBase request)
        {
            return request.ContentType.ToLower().Contains("application/json");
        }
    }
}