namespace NzbDrone.Common.OAuth
{
    public class WebParameter
    {
        public WebParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; set; }
        public string Name { get; private set; }
    }
}
