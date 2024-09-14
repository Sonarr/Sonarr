using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FluentValidation.Validators;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace Sonarr.Api.V3.Config
{
    public static class CertificateValidation
    {
        public static IRuleBuilderOptions<T, string> IsValidCertificate<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CertificateValidator());
        }
    }

    public class CertificateValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Invalid SSL certificate file or password. {message}";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(CertificateValidator));

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            if (context.InstanceToValidate is not HostConfigResource resource)
            {
                return true;
            }

            try
            {
                new X509Certificate2(resource.SslCertPath, resource.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);

                return true;
            }
            catch (CryptographicException ex)
            {
                Logger.Debug(ex, "Invalid SSL certificate file or password. {0}", ex.Message);

                context.MessageFormatter.AppendArgument("message", ex.Message);

                return false;
            }
        }
    }
}
