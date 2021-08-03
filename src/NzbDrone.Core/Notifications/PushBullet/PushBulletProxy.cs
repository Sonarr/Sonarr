using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public interface IPushBulletProxy
    {
        void SendNotification(string title, string message, PushBulletSettings settings);
        List<PushBulletDevice> GetDevices(PushBulletSettings settings);
        ValidationFailure Test(PushBulletSettings settings);
    }

    public class PushBulletProxy : IPushBulletProxy
    {
        private const string PUSH_URL = "https://api.pushbullet.com/v2/pushes";
        private const string DEVICE_URL = "https://api.pushbullet.com/v2/devices";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public PushBulletProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushBulletSettings settings)
        {
            var error = false;

            if (settings.ChannelTags.Any())
            {
                foreach (var channelTag in settings.ChannelTags)
                {
                    var request = BuildChannelRequest(channelTag);

                    try
                    {
                        SendNotification(title, message, request, settings);
                    }
                    catch (PushBulletException ex)
                    {
                        _logger.Error(ex, "Unable to send test message to {0}", channelTag);
                        error = true;
                    }
                }
            }
            else
            {
                if (settings.DeviceIds.Any())
                {
                    foreach (var deviceId in settings.DeviceIds)
                    {
                        var request = BuildDeviceRequest(deviceId);

                        try
                        {
                            SendNotification(title, message, request, settings);
                        }
                        catch (PushBulletException ex)
                        {
                            _logger.Error(ex, "Unable to send test message to {0}", deviceId);
                            error = true;
                        }
                    }
                }
                else
                {
                    var request = BuildDeviceRequest(null);

                    try
                    {
                        SendNotification(title, message, request, settings);
                    }
                    catch (PushBulletException ex)
                    {
                        _logger.Error(ex, "Unable to send test message to all devices");
                        error = true;
                    }
                }
            }

            if (error)
            {
                throw new PushBulletException("Unable to send PushBullet notifications to all channels or devices");
            }
        }

        public List<PushBulletDevice> GetDevices(PushBulletSettings settings)
        {
            try
            {
                var requestBuilder = new HttpRequestBuilder(DEVICE_URL);

                var request = requestBuilder.Build();

                request.Method = HttpMethod.GET;
                request.AddBasicAuthentication(settings.ApiKey, string.Empty);

                var response = _httpClient.Execute(request);

                return Json.Deserialize<PushBulletDevicesResponse>(response.Content).Devices;
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access token is invalid");
                    throw;
                }
            }

            return new List<PushBulletDevice>();
        }

        public ValidationFailure Test(PushBulletSettings settings)
        {
            try
            {
                const string title = "Sonarr - Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", "API Key is invalid");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }

        private HttpRequestBuilder BuildDeviceRequest(string deviceId)
        {
            var requestBuilder = new HttpRequestBuilder(PUSH_URL).Post();
            long integerId;

            if (deviceId.IsNullOrWhiteSpace())
            {
                return requestBuilder;
            }

            if (long.TryParse(deviceId, out integerId))
            {
                requestBuilder.AddFormParameter("device_id", integerId);
            }
            else
            {
                requestBuilder.AddFormParameter("device_iden", deviceId);
            }

            return requestBuilder;
        }

        private HttpRequestBuilder BuildChannelRequest(string channelTag)
        {
            var requestBuilder = new HttpRequestBuilder(PUSH_URL).Post();

            if (channelTag.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("channel_tag", channelTag);
            }

            return requestBuilder;
        }

        private void SendNotification(string title, string message, HttpRequestBuilder requestBuilder, PushBulletSettings settings)
        {
            try
            {
                requestBuilder.AddFormParameter("type", "note")
                    .AddFormParameter("title", title)
                    .AddFormParameter("body", message);

                if (settings.SenderId.IsNotNullOrWhiteSpace())
                {
                    requestBuilder.AddFormParameter("source_device_iden", settings.SenderId);
                }

                var request = requestBuilder.Build();

                request.AddBasicAuthentication(settings.ApiKey, string.Empty);

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access token is invalid");
                    throw;
                }

                throw new PushBulletException("Unable to send text message: {0}", ex, ex.Message);
            }
        }
    }
}
