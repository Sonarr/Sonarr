namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderMessage
    {
        public string Message { get; set; }
        public ProviderMessageType Type { get; set; }

        public ProviderMessage(string message, ProviderMessageType type)
        {
            Message = message;
            Type = type;
        }
    }

    public enum ProviderMessageType
    {
        Info,
        Warning,
        Error
    }
}
