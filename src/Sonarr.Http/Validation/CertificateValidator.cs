using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentValidation.Validators;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation;

namespace Sonarr.Http.Validation;

public class CertificateValidator : PropertyValidator
{
    protected override string GetDefaultMessageTemplate() => "Invalid SSL certificate file or {passwordOrKey}. {message}";

    private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(CertificateValidator));

    protected override bool IsValid(PropertyValidatorContext context)
    {
        if (context.PropertyValue == null)
        {
            return false;
        }

        if (context.InstanceToValidate is not ISslCertificateResource resource)
        {
            return true;
        }

        try
        {
            SslCertificateLoader.LoadCertificateContext(resource.SslCertPath, resource.SslKeyPath, resource.SslCertPassword);

            return true;
        }
        catch (CryptographicException ex)
        {
            var type = X509Certificate2.GetCertContentType(resource.SslCertPath!);
            if (type != X509ContentType.Cert && type != X509ContentType.Pkcs12)
            {
                Logger.Debug("Invalid SSL certificate file. Unexpected certificate type: {0}", type);
                context.MessageFormatter.AppendArgument("passwordOrKey", "password");

                return false;
            }

            var passwordOrKey = type == X509ContentType.Cert ? "key" : "password";

            Logger.Debug(ex, "Invalid SSL certificate file or {0}. {1}", passwordOrKey, ex.Message);

            context.MessageFormatter.AppendArgument("passwordOrKey", passwordOrKey);
            context.MessageFormatter.AppendArgument("message", ex.Message);

            return false;
        }
    }
}
