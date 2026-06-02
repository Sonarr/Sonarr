using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V5.Settings;

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
    public void Validate_fails_when_cert_path_is_null()
    {
        var resource = new GeneralSettingsResource { SslCertPath = null };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
    }

    [Test]
    public void Validate_passes_for_valid_pem_certificate()
    {
        var resource = new GeneralSettingsResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.LeafKeyPath
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_passes_for_valid_pkcs12_certificate()
    {
        var resource = new GeneralSettingsResource
        {
            SslCertPath = _certs.PfxPath,
            SslCertPassword = SslTestCertificates.PfxPassword
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_fails_for_pem_with_wrong_key()
    {
        var resource = new GeneralSettingsResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.WrongKeyPath
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("key"));
    }

    [Test]
    public void Validate_fails_for_pkcs12_with_wrong_password()
    {
        var resource = new GeneralSettingsResource
        {
            SslCertPath = _certs.PfxPath,
            SslCertPassword = "wrong-password"
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("password"));
    }
}
