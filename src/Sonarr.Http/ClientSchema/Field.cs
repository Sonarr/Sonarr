using System.Collections.Generic;

namespace Sonarr.Http.ClientSchema
{
    public class Field
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public string HelpText { get; set; }
        public string HelpLink { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public bool Advanced { get; set; }
        public List<SelectOption> SelectOptions { get; set; }

        public Field Clone()
        {
            return (Field)MemberwiseClone();
        }
    }
}
