using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NzbDrone.Test.Common;

public class SslTestCertificates : IDisposable
{
    public const string PfxPassword = "test-password";

    public string TempDir { get; }
    public string ChainPemPath { get; }
    public string LeafOnlyPemPath { get; }
    public string LeafKeyPath { get; }
    public string WrongKeyPath { get; }
    public string PfxPath { get; }
    public string PfxNoKeyPath { get; }
    public string LeafSerialNumber { get; }
    public string IntermediateSerialNumber { get; }

    public SslTestCertificates()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"sonarr-ssl-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);

        using var rootKey = RSA.Create(2048);
        var rootReq = new CertificateRequest("CN=Test Root CA", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using var rootCert = rootReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));

        using var interKey = RSA.Create(2048);
        var interReq = new CertificateRequest("CN=Test Intermediate CA", interKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        interReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using var interCertPublic = interReq.Create(rootCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5), BitConverter.GetBytes(1));
        using var interCert = interCertPublic.CopyWithPrivateKey(interKey);
        IntermediateSerialNumber = interCert.SerialNumber;

        using var leafKey = RSA.Create(2048);
        var leafReq = new CertificateRequest("CN=Test Leaf", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var leafCertPublic = leafReq.Create(interCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), BitConverter.GetBytes(2));
        using var leafCert = leafCertPublic.CopyWithPrivateKey(leafKey);
        LeafSerialNumber = leafCert.SerialNumber;

        ChainPemPath = Path.Combine(TempDir, "chain.pem");
        File.WriteAllText(ChainPemPath,
            leafCert.ExportCertificatePem() + "\n" +
            interCert.ExportCertificatePem() + "\n" +
            rootCert.ExportCertificatePem());

        LeafOnlyPemPath = Path.Combine(TempDir, "leaf.pem");
        File.WriteAllText(LeafOnlyPemPath, leafCert.ExportCertificatePem());

        LeafKeyPath = Path.Combine(TempDir, "leaf.key");
        File.WriteAllText(LeafKeyPath, leafKey.ExportRSAPrivateKeyPem());

        using var wrongKey = RSA.Create(2048);
        WrongKeyPath = Path.Combine(TempDir, "wrong.key");
        File.WriteAllText(WrongKeyPath, wrongKey.ExportRSAPrivateKeyPem());

        PfxPath = Path.Combine(TempDir, "cert.pfx");
        File.WriteAllBytes(PfxPath, leafCert.Export(X509ContentType.Pkcs12, PfxPassword));

        var noPkeyCollection = new X509Certificate2Collection(X509CertificateLoader.LoadCertificate(leafCert.RawData));
        PfxNoKeyPath = Path.Combine(TempDir, "cert-no-key.pfx");
        File.WriteAllBytes(PfxNoKeyPath, noPkeyCollection.Export(X509ContentType.Pkcs12, "") ?? []);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, recursive: true);
        }
    }
}
