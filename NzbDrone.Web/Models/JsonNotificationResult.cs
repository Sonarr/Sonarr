using System.Web.Mvc;

namespace NzbDrone.Web.Models
{
    public class JsonNotificationResult
    {
        private JsonNotificationResult()
        {
            Text = string.Empty;
        }

        public string Title { get; set; }
        public string Text { get; set; }
        public NotificationType NotificationType { get; set; }


        public static JsonResult Info(string title, string text)
        {
            return GetJsonResult(NotificationType.Info, title, text);
        }

        public static JsonResult Info(string title)
        {
            return GetJsonResult(NotificationType.Info, title, string.Empty);
        }

        public static JsonResult Error(string title, string text)
        {
            return GetJsonResult(NotificationType.Error, title, text);
        }

        public static JsonResult Oops(string text)
        {
            return GetJsonResult(NotificationType.Error, "Oops!", text);
        }

        public static JsonResult Queued(string task)
        {
            return GetJsonResult(NotificationType.Info, "Added to queue", string.Format("{0} will start soon.", task.Trim()));
        }


        public static JsonResult GetJsonResult(NotificationType notificationType, string title, string text)
        {
            return new JsonResult
            {
                Data = new JsonNotificationResult { NotificationType = notificationType, Title = title, Text = text },
                ContentType = "application/json; charset=UTF-8",
                ContentEncoding = System.Text.Encoding.UTF8,
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