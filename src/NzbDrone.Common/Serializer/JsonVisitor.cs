using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Common.Serializer
{
    public class JsonVisitor
    {
        protected void Dispatch(JToken json)
        {
            switch (json.Type)
            {
                case JTokenType.Object:
                    Visit(json as JObject);
                    break;

                case JTokenType.Array:
                    Visit(json as JArray);
                    break;

                case JTokenType.Raw:
                    Visit(json as JRaw);
                    break;

                case JTokenType.Constructor:
                    Visit(json as JConstructor);
                    break;

                case JTokenType.Property:
                    Visit(json as JProperty);
                    break;

                case JTokenType.Comment:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    Visit(json as JValue);
                    break;

                default:
                    break;
            }
        }

        public virtual void Visit(JToken json)
        {
            Dispatch(json);
        }

        public virtual void Visit(JContainer json)
        {
            Dispatch(json);
        }

        public virtual void Visit(JArray json)
        {
            foreach (JToken token in json)
            {
                Visit(token);
            }
        }

        public virtual void Visit(JConstructor json)
        {
        }

        public virtual void Visit(JObject json)
        {
            foreach (JProperty property in json.Properties())
            {
                Visit(property);
            }
        }

        public virtual void Visit(JProperty property)
        {
            Visit(property.Value);
        }

        public virtual void Visit(JValue value)
        {
        }
    }
}
