using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class SearchDefinitionFixture : CoreTest<SingleEpisodeSearchCriteria>
    {
        [TestCase("Betty White's Off Their Rockers", "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Star Wars: The Clone Wars", "Star+Wars+The+Clone+Wars")]
        [TestCase("Hawaii Five-0", "Hawaii+Five+0")]
        [TestCase("Franklin & Bash", "Franklin+and+Bash")]
        [TestCase("Chicago P.D.", "Chicago+PD")]
        [TestCase("Kourtney And Khlo\u00E9 Take The Hamptons", "Kourtney+And+Khloe+Take+The+Hamptons")]
        public void should_replace_some_special_characters(string input, string expected)
        {
            Subject.SceneTitles = new List<string> { input };
            Subject.CleanSceneTitles.First().Should().Be(expected);
        }
    }
}
