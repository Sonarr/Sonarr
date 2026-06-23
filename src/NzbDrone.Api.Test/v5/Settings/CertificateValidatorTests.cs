using FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V5.Settings;
using Sonarr.Http.Validation;

namespace NzbDrone.Api.Test.v5.Settings;

[TestFixture]
public class CertificateValidatorTests
{
    private SslTestCertificates _certs;

    private class GeneralSettingsCertValidator : AbstractValidator<GeneralSettingsResource>
    {
        public GeneralSettingsCertValidator()
        {
            RuleFor(x => x.SslCertPath).IsValidCertificate();
        }
    }

    private readonly GeneralSettingsCertValidator _validator = new();

    [OneTimeSetUp]
    public void CreateTestCertificates()
    {
        _certs = new SslTestCertificates();
    }

    [OneTimeTearDown]
    public void DeleteTestCertificates()
    {
        _certs?.Dispose();
    }

    [Test]
    public void general_settings_resource_conforms_to_certificate_validation()
    {
        _validator.TestValidate(new GeneralSettingsResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.LeafKeyPath
        }).ShouldNotHaveValidationErrorFor(r => r.SslCertPath);

        _validator.TestValidate(new GeneralSettingsResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.WrongKeyPath
        }).ShouldHaveValidationErrorFor(r => r.SslCertPath);
    }
}
