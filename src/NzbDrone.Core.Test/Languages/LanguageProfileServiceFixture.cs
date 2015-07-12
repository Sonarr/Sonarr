using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.Languages
{
    [TestFixture]

    public class LanguageProfileServiceFixture : CoreTest<LanguageProfileService>
    {
        [Test]
        public void init_should_add_default_profiles()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<ILanguageProfileRepository>()
                  .Verify(v => v.Insert(It.IsAny<LanguageProfile>()), Times.Once());
        }

        [Test]
        //This confirms that new profiles are added only if no other profiles exists.
        //We don't want to keep adding them back if a user deleted them on purpose.
        public void Init_should_skip_if_any_profiles_already_exist()
        {
            Mocker.GetMock<ILanguageProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(Builder<LanguageProfile>.CreateListOfSize(2).Build().ToList());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<ILanguageProfileRepository>()
                  .Verify(v => v.Insert(It.IsAny<LanguageProfile>()), Times.Never());
        }


        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_series()
        {
            var seriesList = Builder<Series>.CreateListOfSize(3)
                                            .Random(1)
                                            .With(c => c.LanguageProfileId = 2)
                                            .Build().ToList();


            Mocker.GetMock<ISeriesService>().Setup(c => c.GetAllSeries()).Returns(seriesList);

            Assert.Throws<LanguageProfileInUseException>(() => Subject.Delete(2));

            Mocker.GetMock<ILanguageProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());

        }


        [Test]
        public void should_delete_profile_if_not_assigned_to_series()
        {
            var seriesList = Builder<Series>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.LanguageProfileId = 2)
                                            .Build().ToList();


            Mocker.GetMock<ISeriesService>().Setup(c => c.GetAllSeries()).Returns(seriesList);

            Subject.Delete(1);

            Mocker.GetMock<ILanguageProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }
    }
}