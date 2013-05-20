namespace NzbDrone.Api.ClientSchema
{
    public class Field
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string HelpText { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}