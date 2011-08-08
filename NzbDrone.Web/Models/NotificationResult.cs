namespace NzbDrone.Web.Models
{
    public class NotificationResult
    {
        public NotificationResult()
        {
            Text = string.Empty;
        }

        public bool IsMessage { get { return true; } }

        public string Title { get; set; }
        public string Text { get; set; }
        public NotificationType NotificationType { get; set; }


    }

    public enum NotificationType
    {
        Info,
        Error
    }

}