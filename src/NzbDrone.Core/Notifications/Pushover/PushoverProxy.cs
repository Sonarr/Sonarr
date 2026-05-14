using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Pushover
{
    public interface IPushoverProxy
    {
        void SendNotification(string title, string message, PushoverSettings settings);
        ValidationFailure Test(PushoverSettings settings);
    }

    public class PushoverProxy : IPushoverProxy
    {
        private const string URL = "https://api.pushover.net/1/messages.json";

        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public PushoverProxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushoverSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(URL).Post();

            var encrypted = settings.EncryptionKey.IsNotNullOrWhiteSpace();

            if (encrypted)
            {
                var key = HexStringToBytes(settings.EncryptionKey);
                title = EncryptField(title, key);
                message = EncryptField(message, key);
            }

            requestBuilder.AddFormParameter("token", settings.ApiKey)
                          .AddFormParameter("user", settings.UserKey)
                          .AddFormParameter("device", string.Join(",", settings.Devices))
                          .AddFormParameter("title", title)
                          .AddFormParameter("message", message)
                          .AddFormParameter("priority", settings.Priority);

            if (encrypted)
            {
                requestBuilder.AddFormParameter("encrypted", 1);
            }

            if ((PushoverPriority)settings.Priority == PushoverPriority.Emergency)
            {
                requestBuilder.AddFormParameter("retry", settings.Retry);
                requestBuilder.AddFormParameter("expire", settings.Expire);
            }

            if (settings.Ttl > 0)
            {
                requestBuilder.AddFormParameter("ttl", settings.Ttl);
            }

            if (!settings.Sound.IsNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("sound", settings.Sound);
            }

            var request = requestBuilder.Build();

            _httpClient.Post(request);
        }

        public ValidationFailure Test(PushoverSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private static string EncryptField(string plaintext, byte[] key)
        {
            var compressed = GzipCompress(Encoding.UTF8.GetBytes(plaintext ?? string.Empty));

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.GenerateIV();

            var iv = aes.IV;
            byte[] ciphertext;

            using (var encryptor = aes.CreateEncryptor())
            {
                ciphertext = encryptor.TransformFinalBlock(compressed, 0, compressed.Length);
            }

            var ivAndCiphertext = new byte[iv.Length + ciphertext.Length];
            Buffer.BlockCopy(iv, 0, ivAndCiphertext, 0, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, ivAndCiphertext, iv.Length, ciphertext.Length);

            using var hmac = new HMACSHA256(key);
            var mac = hmac.ComputeHash(ivAndCiphertext);

            var payload = new byte[ivAndCiphertext.Length + mac.Length];
            Buffer.BlockCopy(ivAndCiphertext, 0, payload, 0, ivAndCiphertext.Length);
            Buffer.BlockCopy(mac, 0, payload, ivAndCiphertext.Length, mac.Length);

            return Convert.ToBase64String(payload);
        }

        private static byte[] GzipCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        private static byte[] HexStringToBytes(string hex)
        {
            var bytes = new byte[hex.Length / 2];

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
