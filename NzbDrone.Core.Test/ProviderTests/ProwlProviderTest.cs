using System;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using Prowlin;

// ReSharper disable InconsistentNaming

namespace NzbDrone.Core.Test.ProviderTests
{
    [Explicit]
    [TestFixture]
    public class ProwlProviderTest : TestBase
    {
        private const string _apiKey = "c3bdc0f48168f72d546cc6872925b160f5cbffc1";
        private const string _apiKey2 = "46a710a46b111b0b8633819b0d8a1e0272a3affa";

        private const string _badApiKey = "1234567890abcdefghijklmnopqrstuvwxyz1234";

        [Test]
        public void Verify_should_return_true_for_a_valid_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            
            //Act
            var result = mocker.Resolve<ProwlProvider>().Verify(_apiKey);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Verify_should_return_false_for_an_invalid_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().Verify(_badApiKey);

            //Assert
            ExceptionVerification.ExcpectedWarns(1);
            result.Should().BeFalse();
        }

        [Test]
        public void SendNotification_should_return_true_for_a_valid_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_false_for_an_invalid_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _badApiKey);

            //Assert
            ExceptionVerification.ExcpectedWarns(1);
            result.Should().BeFalse();
        }

        [Test]
        public void SendNotification_should_alert_with_high_priority()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone (High)", _apiKey, NotificationPriority.High);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_alert_with_VeryLow_priority()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone (VeryLow)", _apiKey, NotificationPriority.VeryLow);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_have_a_call_back_url()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey, NotificationPriority.Normal, "http://www.nzbdrone.com");

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_true_for_two_valid_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey + ", " + _apiKey2);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SendNotification_should_return_true_for_valid_apiKey_with_bad_apiKey()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<ProwlProvider>().SendNotification("NzbDrone Test", "This is a test message from NzbDrone", _apiKey + ", " + _badApiKey);

            //Assert
            result.Should().BeTrue();
        }
    }
}