using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Localization
{
    [TestFixture]
    public class LocalizationServiceFixture : CoreTest<LocalizationService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().Setup(m => m.UILanguage).Returns((int)Language.English);

            Mocker.GetMock<IAppFolderInfo>().Setup(m => m.StartUpFolder).Returns(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void should_get_string_in_dictionary_if_lang_exists_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("UI Language");

            localizedString.Should().Be("UI Language");
        }

        [Test]
        public void should_get_string_in_default_language_dictionary_if_no_lang_country_code_exists_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("UI Language", "fr_fr");

            localizedString.Should().Be("UI Langue");

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_get_string_in_default_dictionary_if_no_lang_exists_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("UI Language", "an");

            localizedString.Should().Be("UI Language");

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_get_string_in_default_dictionary_if_lang_empty_and_string_exists()
        {
            var localizedString = Subject.GetLocalizedString("UI Language", "");

            localizedString.Should().Be("UI Language");
        }

        [Test]
        public void should_return_argument_if_string_doesnt_exists()
        {
            var localizedString = Subject.GetLocalizedString("badString", "en");

            localizedString.Should().Be("badString");
        }

        [Test]
        public void should_return_argument_if_string_doesnt_exists_default_lang()
        {
            var localizedString = Subject.GetLocalizedString("badString");

            localizedString.Should().Be("badString");
        }

        [Test]
        public void should_throw_if_empty_string_passed()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetLocalizedString(""));
        }

        [Test]
        public void should_throw_if_null_string_passed()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetLocalizedString(null));
        }
    }
}
