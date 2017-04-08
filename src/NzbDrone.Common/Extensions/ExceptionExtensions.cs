using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static T WithData<T>(this T ex, string key, string value) where T : Exception
        {
            ex.AddData(key, value);

            return ex;
        }
        public static T WithData<T>(this T ex, string key, int value) where T : Exception
        {
            ex.AddData(key, value.ToString());

            return ex;
        }

        public static T WithData<T>(this T ex, string key, Http.HttpUri value) where T : Exception
        {
            ex.AddData(key, value.ToString());

            return ex;
        }


        public static T WithData<T>(this T ex, Http.HttpResponse response, int maxSampleLength = 512) where T : Exception
        {
            if (response == null || response.Content == null) return ex;

            var contentSample = response.Content.Substring(0, Math.Min(response.Content.Length, 512));

            if (response.Headers != null)
            {
                ex.AddData("ContentType", response.Headers.ContentType ?? string.Empty);
            }
            ex.AddData("ContentLength", response.Content.Length.ToString());
            ex.AddData("ContentSample", contentSample);

            return ex;
        }


        private static void AddData(this Exception ex, string key, string value)
        {
            if (value.IsNullOrWhiteSpace()) return;

            ex.Data[key] = value;
        }
    }
}
