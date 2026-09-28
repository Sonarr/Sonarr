using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.ValidationTests
{
    [TestFixture]
    public class ValidHostFixture
    {
        private HostValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new HostValidator();
        }

        [TestCase("localhost")]
        [TestCase("qbittorrent.lan")]
        [TestCase("192.168.0.1")]
        [TestCase("::1")]
        [TestCase("[::1]")]
        [TestCase("fd3a:9a29:b136:eeee:aaaa::6")]
        [TestCase("[fd3a:9a29:b136:eeee:aaaa::6]")]
        public void should_be_valid_host(string host)
        {
            _validator.Validate(new HostSettings { Host = host }).IsValid.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase("http://localhost")]
        [TestCase("localhost:8080")]
        [TestCase("[localhost]")]
        [TestCase("[192.168.0.1]")]
        public void should_not_be_valid_host(string host)
        {
            _validator.Validate(new HostSettings { Host = host }).IsValid.Should().BeFalse();
        }

        private class HostSettings
        {
            public string Host { get; set; }
        }

        private class HostValidator : AbstractValidator<HostSettings>
        {
            public HostValidator()
            {
                RuleFor(c => c.Host).ValidHost();
            }
        }
    }
}
