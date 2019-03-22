using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using RestSharp.Authenticators;

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
        private readonly Logger _logger;
        private const string PUSH_URL = "https://api.pushbullet.com/v2/pushes";
        private const string DEVICE_URL = "https://api.pushbullet.com/v2/devices";

        public PushBulletProxy(Logger logger)
        {
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
                var client = RestClientFactory.BuildClient(DEVICE_URL);
                var request = new RestRequest(Method.GET);

                client.Authenticator = new HttpBasicAuthenticator(settings.ApiKey, string.Empty);
                var response = client.ExecuteAndValidate(request);

                return Json.Deserialize<PushBulletDevicesResponse>(response.Content).Devices;
            }
            catch (RestException ex)
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
            catch (RestException ex)
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

        private RestRequest BuildDeviceRequest(string deviceId)
        {
            var request = new RestRequest(Method.POST);
            long integerId;

            if (long.TryParse(deviceId, out integerId))
            {
                request.AddParameter("device_id", integerId);
            }

            else
            {
                request.AddParameter("device_iden", deviceId);
            }

            return request;
        }

        private RestRequest BuildChannelRequest(string channelTag)
        {
            var request = new RestRequest(Method.POST);
            request.AddParameter("channel_tag", channelTag);

            return request;
        }

        private void SendNotification(string title, string message, RestRequest request, PushBulletSettings settings)
        {
            try
            {
                var client = RestClientFactory.BuildClient(PUSH_URL);

                request.AddParameter("type", "note");
                request.AddParameter("title", title);
                request.AddParameter("body", message);

                if (settings.SenderId.IsNotNullOrWhiteSpace())
                {
                    request.AddParameter("source_device_iden", settings.SenderId);
                }

                client.Authenticator = new HttpBasicAuthenticator(settings.ApiKey, string.Empty);
                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
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
