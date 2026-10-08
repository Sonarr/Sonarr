using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class UserServiceFixture : CoreTest<UserService>
    {
        private User _user;

        [SetUp]
        public void Setup()
        {
            _user = new User
            {
                Identifier = Guid.NewGuid(),
                Username = "admin",
                Password = "stored-hash",
                Salt = "c2FsdA==",
                Iterations = 10000
            };

            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.SingleOrDefault())
                  .Returns(() => _user);

            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.Update(It.IsAny<User>()))
                  .Returns<User>(u => u);
        }

        [Test]
        public void upsert_should_keep_password_when_password_is_null()
        {
            var user = Subject.Upsert("NewAdmin", null);

            user.Username.Should().Be("newadmin");
            user.Password.Should().Be("stored-hash");
            user.Salt.Should().Be("c2FsdA==");
        }

        [Test]
        public void upsert_should_hash_new_password()
        {
            var user = Subject.Upsert("admin", "new-password");

            user.Password.Should().NotBe("stored-hash");
            user.Password.Should().NotBe("new-password");
            user.Salt.Should().NotBe("c2FsdA==");
        }

        [Test]
        public void upsert_should_hash_password_that_matches_stored_hash()
        {
            var user = Subject.Upsert("admin", "stored-hash");

            user.Password.Should().NotBe("stored-hash");
        }

        [Test]
        public void upsert_should_add_user_when_none_exists()
        {
            _user = null;

            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.Insert(It.IsAny<User>()))
                  .Returns<User>(u => u);

            var user = Subject.Upsert("Admin", "password");

            user.Username.Should().Be("admin");
            user.Password.Should().NotBe("password");
        }
    }
}
