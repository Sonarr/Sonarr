using FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Config;
using Sonarr.Http.Validation;

namespace NzbDrone.Api.Test.v3.Config;

[TestFixture]
public class CertificateValidatorTests : TestBase
{
    private SslTestCertificates _certs;

    private class HostConfigCertValidator : AbstractValidator<HostConfigResource>
    {
        public HostConfigCertValidator()
        {
            RuleFor(x => x.SslCertPath).IsValidCertificate();
        }
    }

    private readonly HostConfigCertValidator _validator = new();

    [OneTimeSetUp]
    public void CreateTestCertificates()
    {
        _certs = new SslTestCertificates(TempFolder);
    }

    [Test]
    public void host_config_resource_conforms_to_certificate_validation()
    {
        _validator.TestValidate(new HostConfigResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.LeafKeyPath
        }).ShouldNotHaveValidationErrorFor(r => r.SslCertPath);

        _validator.TestValidate(new HostConfigResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.WrongKeyPath
        }).ShouldHaveValidationErrorFor(r => r.SslCertPath);
    }
}
