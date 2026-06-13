using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Config;
using Sonarr.Http.Validation;

namespace NzbDrone.Api.Test.v3.Config;

[TestFixture]
public class CertificateValidatorTests
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
        _certs = new SslTestCertificates();
    }

    [OneTimeTearDown]
    public void DeleteTestCertificates()
    {
        _certs?.Dispose();
    }

    [Test]
    public void validate_fails_when_cert_path_is_null()
    {
        var resource = new HostConfigResource { SslCertPath = null };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
    }

    [Test]
    public void validate_passes_for_valid_pem_certificate()
    {
        var resource = new HostConfigResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.LeafKeyPath
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void validate_passes_for_valid_pkcs12_certificate()
    {
        var resource = new HostConfigResource
        {
            SslCertPath = _certs.PfxPath,
            SslCertPassword = SslTestCertificates.PfxPassword
        };

        var result = _validator.TestValidate(resource);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void validate_fails_for_pem_with_wrong_key()
    {
        var resource = new HostConfigResource
        {
            SslCertPath = _certs.LeafOnlyPemPath,
            SslKeyPath = _certs.WrongKeyPath
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("key"));
    }

    [Test]
    public void validate_fails_for_pkcs12_with_wrong_password()
    {
        var resource = new HostConfigResource
        {
            SslCertPath = _certs.PfxPath,
            SslCertPassword = "wrong-password"
        };

        var result = _validator.TestValidate(resource);

        result.ShouldHaveValidationErrorFor(r => r.SslCertPath);
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("password"));
    }
}
