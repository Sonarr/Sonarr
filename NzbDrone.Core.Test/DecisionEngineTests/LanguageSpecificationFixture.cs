// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class LanguageSpecificationFixture : CoreTest
    {
        private EpisodeParseResult parseResult;

        private void WithEnglishRelease()
        {
            parseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Language = LanguageType.English)
                    .Build();
        }

        private void WithGermanRelease()
        {
            parseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Language = LanguageType.German)
                    .Build();
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}