using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class SearchDefinitionFixture : CoreTest<SingleEpisodeSearchCriteria>
    {
        [TestCase("Betty White's Off Their Rockers", Result = "Betty+Whites+Off+Their+Rockers")]
        [TestCase("Star Wars: The Clone Wars", Result = "Star+Wars+The+Clone+Wars")]
        [TestCase("Hawaii Five-0", Result = "Hawaii+Five+0")]
        [TestCase("Franklin & Bash", Result = "Franklin+and+Bash")]
        public string should_replace_some_special_characters(string input)
        {
            Subject.SceneTitle = input;
            return Subject.QueryTitle;
        }
    }
}
