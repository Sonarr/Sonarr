using System;
using System.Collections.Generic;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using NzbDrone.Core.Authentication;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class UpsertOidcUserFixture : CoreTest<UserService>
    {
        private Mock<IUserRepository> Repo => Mocker.GetMock<IUserRepository>();

        [SetUp]
        public void Setup()
        {
            Repo.Setup(r => r.Insert(It.IsAny<User>())).Returns<User>(u => u);
            Repo.Setup(r => r.Update(It.IsAny<User>())).Returns<User>(u => u);
            Repo.Setup(r => r.All()).Returns(new List<User>());
            Repo.Setup(r => r.FindBySubject(It.IsAny<string>())).Returns((User)null);
            Repo.Setup(r => r.FindByUsername(It.IsAny<string>())).Returns((User)null);
            Repo.Setup(r => r.FindUser(It.IsAny<string>())).Returns((User)null);
        }

        [Test]
        public void subject_first_lookup_updates_existing_user()
        {
            var existing = new User
            {
                Id = 1,
                Identifier = Guid.NewGuid(),
                Username = "oldname",
                OidcSubject = "subject-1",
                OidcGroups = "old",
                Role = SonarrRole.ReadOnly
            };

            Repo.Setup(r => r.FindBySubject("subject-1")).Returns(existing);

            var result = Subject.UpsertOidcUser("subject-1", "NewName", "g1,g2", SonarrRole.Manage);

            result.Should().BeSameAs(existing);
            result.Username.Should().Be("newname");
            result.OidcGroups.Should().Be("g1,g2");
            result.Role.Should().Be(SonarrRole.Manage);
            result.OidcSubject.Should().Be("subject-1");

            Repo.Verify(r => r.Update(existing), Times.Once());
            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public void subject_first_lookup_falls_back_to_all_scan()
        {
            var existing = new User
            {
                Id = 2,
                Identifier = Guid.NewGuid(),
                Username = "scanuser",
                OidcSubject = "subject-2",
                Role = SonarrRole.AddSearch
            };

            Repo.Setup(r => r.All()).Returns(new List<User> { existing });

            var result = Subject.UpsertOidcUser("subject-2", "ScanUser", "groups", SonarrRole.Admin);

            result.Should().BeSameAs(existing);
            result.Username.Should().Be("scanuser");
            result.Role.Should().Be(SonarrRole.Admin);
            result.OidcGroups.Should().Be("groups");

            Repo.Verify(r => r.Update(existing), Times.Once());
            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public void forms_user_with_matching_username_is_linked()
        {
            var formsUser = new User
            {
                Id = 3,
                Identifier = Guid.NewGuid(),
                Username = "alice",
                OidcSubject = null,
                Role = SonarrRole.Admin
            };

            Repo.Setup(r => r.FindByUsername("alice")).Returns(formsUser);

            var result = Subject.UpsertOidcUser("subject-3", "ALICE", "groups", SonarrRole.ReadOnly);

            result.Should().BeSameAs(formsUser);
            formsUser.OidcSubject.Should().Be("subject-3");
            formsUser.Username.Should().Be("alice");
            formsUser.OidcGroups.Should().Be("groups");
            formsUser.Role.Should().Be(SonarrRole.ReadOnly);

            Repo.Verify(r => r.Update(formsUser), Times.Once());
            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public void forms_linking_falls_back_to_all_scan_case_insensitive()
        {
            var formsUser = new User
            {
                Id = 4,
                Identifier = Guid.NewGuid(),
                Username = "Bob",
                OidcSubject = null,
                Role = SonarrRole.Admin
            };

            Repo.Setup(r => r.All()).Returns(new List<User> { formsUser });

            var result = Subject.UpsertOidcUser("subject-4", "bob", "groups", SonarrRole.Manage);

            result.Should().BeSameAs(formsUser);
            formsUser.OidcSubject.Should().Be("subject-4");
            formsUser.Username.Should().Be("bob");
            formsUser.Role.Should().Be(SonarrRole.Manage);

            Repo.Verify(r => r.Update(formsUser), Times.Once());
            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public void forms_user_with_existing_subject_is_not_relinked()
        {
            var existingOidcUser = new User
            {
                Id = 5,
                Identifier = Guid.NewGuid(),
                Username = "carol",
                OidcSubject = "other-subject",
                Role = SonarrRole.Admin
            };

            // Forms lookup returns a user that already has a subject -> must not link.
            Repo.Setup(r => r.FindByUsername("carol")).Returns(existingOidcUser);

            // No subject-first match.
            Repo.Setup(r => r.All()).Returns(new List<User> { existingOidcUser });

            var result = Subject.UpsertOidcUser("subject-5", "carol", "groups", SonarrRole.ReadOnly);

            // Falls through to insert (since formsUser.OidcSubject != null).
            result.OidcSubject.Should().Be("subject-5");
            result.Username.Should().Be("carol");

            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Once());
        }

        [Test]
        public void new_user_is_inserted_when_no_match()
        {
            var result = Subject.UpsertOidcUser("subject-6", "NewUser", "groups", SonarrRole.AddSearch);

            result.Should().NotBeNull();
            result.OidcSubject.Should().Be("subject-6");
            result.Username.Should().Be("newuser");
            result.OidcGroups.Should().Be("groups");
            result.Role.Should().Be(SonarrRole.AddSearch);

            Repo.Verify(r => r.Insert(It.IsAny<User>()), Times.Once());
            Repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never());
        }

        [Test]
        public void username_is_normalized_to_lowercase_on_insert()
        {
            var result = Subject.UpsertOidcUser("subject-7", "MixedCaseName", null, SonarrRole.ReadOnly);

            result.Username.Should().Be("mixedcasename");
        }

        [Test]
        public void subject_first_lookup_with_null_subject_skips_to_forms()
        {
            var formsUser = new User
            {
                Id = 8,
                Identifier = Guid.NewGuid(),
                Username = "dave",
                OidcSubject = null,
                Role = SonarrRole.Admin
            };

            Repo.Setup(r => r.FindByUsername("dave")).Returns(formsUser);

            var result = Subject.UpsertOidcUser(null, "Dave", "groups", SonarrRole.Manage);

            result.Should().BeSameAs(formsUser);
            formsUser.OidcSubject.Should().BeNull(); // subject null -> not linked?

            // Actually code: if subject.IsNullOrWhiteSpace, formsUser.OidcSubject = subject (null). Still updates.
            formsUser.Role.Should().Be(SonarrRole.Manage);

            Repo.Verify(r => r.Update(formsUser), Times.Once());
        }
    }
}
