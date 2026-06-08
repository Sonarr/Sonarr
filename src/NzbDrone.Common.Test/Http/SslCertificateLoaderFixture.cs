using System;
using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http;

[TestFixture]
public class SslCertificateLoaderFixture
{
    private SslTestCertificates _certs;

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
    public void exception_is_CryptographicException()
    {
        var ex = new SslCertificateLoadException("test");

        ex.Should().BeAssignableTo<CryptographicException>();
    }

    [Test]
    public void exception_preserves_message()
    {
        var ex = new SslCertificateLoadException("expected message");

        ex.Message.Should().Be("expected message");
    }

    [Test]
    public void loadCertificateContext_loads_pem_with_separate_key()
    {
        var context = SslCertificateLoader.LoadCertificateContext(_certs.LeafOnlyPemPath, _certs.LeafKeyPath, null);

        context.Should().NotBeNull();
        context.TargetCertificate.SerialNumber.Should().Be(_certs.LeafSerialNumber);
        context.TargetCertificate.HasPrivateKey.Should().BeTrue();
    }

    [Test]
    public void loadCertificateContext_loads_pem_chain_with_intermediate()
    {
        var context = SslCertificateLoader.LoadCertificateContext(_certs.ChainPemPath, _certs.LeafKeyPath, null);

        context.Should().NotBeNull();
        context.TargetCertificate.SerialNumber.Should().Be(_certs.LeafSerialNumber);
    }

    [Test]
    public void loadCertificateContext_includes_intermediate_in_chain()
    {
        var context = SslCertificateLoader.LoadCertificateContext(_certs.ChainPemPath, _certs.LeafKeyPath, null);

        context.IntermediateCertificates.Should().Contain(c => c.SerialNumber == _certs.IntermediateSerialNumber);
    }

    [Test]
    public void loadCertificateContext_removes_duplicate_leaf_from_chain()
    {
        var context = SslCertificateLoader.LoadCertificateContext(_certs.LeafOnlyPemPath, _certs.LeafKeyPath, null);

        context.IntermediateCertificates.Should().BeEmpty();
    }

    [Test]
    public void loadCertificateContext_loads_pkcs12()
    {
        var context = SslCertificateLoader.LoadCertificateContext(_certs.PfxPath, null, SslTestCertificates.PfxPassword);

        context.Should().NotBeNull();
        context.TargetCertificate.SerialNumber.Should().Be(_certs.LeafSerialNumber);
        context.TargetCertificate.HasPrivateKey.Should().BeTrue();
    }

    [Test]
    public void loadCertificateContext_throws_when_pkcs12_has_no_private_key()
    {
        Action act = () => SslCertificateLoader.LoadCertificateContext(_certs.PfxNoKeyPath, null, "");

        act.Should().Throw<SslCertificateLoadException>()
            .WithMessage("*private key*");
    }

    [Test]
    public void loadCertificateContext_throws_for_wrong_pem_key()
    {
        Action act = () => SslCertificateLoader.LoadCertificateContext(_certs.LeafOnlyPemPath, _certs.WrongKeyPath, null);

        act.Should().Throw<CryptographicException>();
    }
}
