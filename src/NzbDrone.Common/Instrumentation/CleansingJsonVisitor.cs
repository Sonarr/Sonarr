using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Instrumentation
{
    public class CleansingJsonVisitor : JsonVisitor
    {
        public override void Visit(JArray json)
        {
            for (var i = 0; i < json.Count; i++)
            {
                if (json[i].Type == JTokenType.String)
                {
                    var text = json[i].Value<string>();
                    json[i] = new JValue(CleanseLogMessage.Cleanse(text));
                }
            }

            foreach (var token in json)
            {
                Visit(token);
            }
        }

        public override void Visit(JProperty property)
        {
            if (property.Value.Type == JTokenType.String)
            {
                property.Value = CleanseValue(property.Value as JValue);
            }
            else
            {
                base.Visit(property);
            }
        }

        private JValue CleanseValue(JValue value)
        {
            var text = value.Value<string>();
            var cleansed = CleanseLogMessage.Cleanse(text);
            return new JValue(cleansed);
        }
    }
}
