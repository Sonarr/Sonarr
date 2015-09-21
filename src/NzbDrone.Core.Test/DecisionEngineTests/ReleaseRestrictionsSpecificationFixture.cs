﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.Common;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ReleaseRestrictionsSpecificationFixture : CoreTest<ReleaseRestrictionsSpecification>
    {
        private RemoteEpisode _remoteEpisode;
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
                           {
                               Series = new Series
                                        {
                                            Tags = new HashSet<Int32>()
                                        },
                               Release = new ReleaseInfo
                                         {
                                             Title = "Dexter.S08E01.EDITED.WEBRip.x264-KYR"
                                         }
                           };

            _remoteMovie = new RemoteMovie
            {
                Movie = new Movie
                {
                    Tags = new HashSet<Int32>()
                },
                Release = new ReleaseInfo
                {
                    Title = "Zipper.2015.EDITED.WEBRip.x264-KYR"
                }
            };
        }

        private void GivenRestictions(String required, String ignored)
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<Int32>>()))
                  .Returns(new List<Restriction>
                           {
                               new Restriction
                               {
                                   Required = required,
                                   Ignored = ignored
                               }
                           });
        }

        [Test]
        public void should_be_true_when_restrictions_are_empty()
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<Int32>>()))
                  .Returns(new List<Restriction>());

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_title_contains_one_required_term()
        {
            GivenRestictions("WEBRip", null);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_does_not_contain_any_required_terms()
        {
            GivenRestictions("doesnt,exist", null);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_title_does_not_contain_any_ignored_terms()
        {
            GivenRestictions(null, "ignored");

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_contains_one_anded_ignored_terms()
        {
            GivenRestictions(null, "edited");

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_required(String required)
        {
            GivenRestictions(required, null);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_ignored(String ignored)
        {
            GivenRestictions(null, ignored);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();

        }

        [Test]
        public void should_be_false_when_release_contains_one_restricted_word_and_one_required_word()
        {
            _remoteEpisode.Release.Title = "[ www.Speed.cd ] -Whose.Line.is.it.Anyway.US.S10E24.720p.HDTV.x264-BAJSKORV";
            _remoteMovie.Release.Title = "[ www.Speed.cd ] -Whose.Line.is.it.Anyway.US.S10E24.720p.HDTV.x264-BAJSKORV";

            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<Int32>>()))
                  .Returns(new List<Restriction>
                           {
                               new Restriction { Required = "x264", Ignored = "www.Speed.cd" }
                           });

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}
