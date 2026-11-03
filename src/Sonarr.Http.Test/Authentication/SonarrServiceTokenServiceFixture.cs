using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using NUnit.Framework;

using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using NzbDrone.Test.Common;

using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class SonarrServiceTokenServiceFixture : TestBase<SonarrServiceTokenService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .SetupGet(c => c.AppDataFolder)
                  .Returns(TempFolder);
        }

        private byte[] ReadKey()
        {
            var path = Path.Combine(TempFolder, "service-token.key");
            var content = File.ReadAllText(path).Trim();
            try
            {
                return Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                return Encoding.UTF8.GetBytes(content);
            }
        }

        [Test]
        public void should_roundtrip_minted_token_with_sub_and_role()
        {
            var token = Subject.MintToken("alice", SonarrRole.Manage);
            var principal = Subject.ValidateToken(token);

            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.FindFirst("sub")?.Value, Is.EqualTo("alice"));
            Assert.That(principal.FindFirst("role")?.Value, Is.EqualTo("Manage"));
        }

        [Test]
        public void should_reject_expired_token()
        {
            GC.KeepAlive(Subject);
            var key = ReadKey();

            var handler = new JwtSecurityTokenHandler();
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: "sonarr",
                audience: "sonarr",
                claims: new[]
                {
                    new Claim("sub", "alice"),
                    new Claim("role", "Admin"),
                },
                notBefore: DateTime.UtcNow.AddMinutes(-20),
                expires: DateTime.UtcNow.AddMinutes(-10),
                signingCredentials: credentials);
            var token = handler.WriteToken(jwt);

            Assert.That(Subject.ValidateToken(token), Is.Null);
        }

        [Test]
        public void should_reject_wrong_audience()
        {
            GC.KeepAlive(Subject);
            var key = ReadKey();

            var handler = new JwtSecurityTokenHandler();
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: "sonarr",
                audience: "wrong",
                claims: new[]
                {
                    new Claim("sub", "alice"),
                    new Claim("role", "Admin"),
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);
            var token = handler.WriteToken(jwt);

            Assert.That(Subject.ValidateToken(token), Is.Null);
        }

        [Test]
        public void should_reject_tampered_signature()
        {
            var token = Subject.MintToken("alice", SonarrRole.Admin);
            var tampered = token[..^4] + "AAAA";
            Assert.That(Subject.ValidateToken(tampered), Is.Null);
        }

        [Test]
        public void should_throw_when_key_file_is_too_short()
        {
            var path = Path.Combine(TempFolder, "service-token.key");
            File.WriteAllText(path, Convert.ToBase64String(new byte[16]));

            Assert.Throws<InvalidOperationException>(() =>
            {
                var unused = new SonarrServiceTokenService(Mocker.GetMock<IAppFolderInfo>().Object);
            });
        }

        [Test]
        public void should_map_permissions_for_all_roles()
        {
            Assert.That(SonarrRolePermissions.ForRole(SonarrRole.ReadOnly), Is.EqualTo(new[] { "UI.Read" }));
            Assert.That(SonarrRolePermissions.ForRole(SonarrRole.AddSearch), Is.EqualTo(new[] { "UI.Read", "UI.Add" }));
            Assert.That(SonarrRolePermissions.ForRole(SonarrRole.Manage), Is.EqualTo(new[] { "UI.Read", "UI.Add", "UI.Manage" }));
            Assert.That(SonarrRolePermissions.ForRole(SonarrRole.Admin), Is.EqualTo(new[] { "UI.Read", "UI.Add", "UI.Manage", "UI.Admin" }));
        }

        [Test]
        public void should_mint_token_that_validates_with_bearer_claims_mapped()
        {
            var token = Subject.MintToken("alice", SonarrRole.Manage);
            var principal = Subject.ValidateToken(token);

            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.FindFirst("sub")?.Value, Is.EqualTo("alice"));
            Assert.That(principal.FindFirst("role")?.Value, Is.EqualTo("Manage"));
        }
    }
}
