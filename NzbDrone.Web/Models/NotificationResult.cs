using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class NotificationResult
    {
        public NotificationResult()
        {
            Text = string.Empty;
        }

        public string Title { get; set; }
        public string Text { get; set; }
        public NotificationType NotificationType { get; set; }


        public static JsonResult Info(string title, string text)
        {
            return GetJsonResult(NotificationType.Error, title, text);
        }

        public static JsonResult Error(string title, string text)
        {
            return GetJsonResult(NotificationType.Error, title, text);
        }

        public static JsonResult GetJsonResult(NotificationType notificationType, string title, string text)
        {
            return new JsonResult
            {
                Data = new NotificationResult { NotificationType = notificationType, Title = title, Text = text },
                ContentType = null,
                ContentEncoding = null,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

    public enum NotificationType
    {
        Info,
        Error
    }

}