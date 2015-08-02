using System.Collections.Generic;
using System.Linq;
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
        [TestCase("Kourtney And Khloé Take The Hamptons", Result = "Kourtney+And+Khloe+Take+The+Hamptons")]
        public string should_replace_some_special_characters(string input)
        {
            Subject.SceneTitles = new List<string> { input };
            if (Subject.QueryTitles.Count > 1)
            {
                return "Too many titles";
            }

            return Subject.QueryTitles.First();
        }

        [TestCase("Chicago P.D.", Result = "Chicago+P.D.|Chicago+PD")]
        [TestCase("Aldnoah.Zero", Result = "Aldnoah.Zero|AldnoahZero")]
        [TestCase("R.O.D Read Or Die", Result = "R.O.D+Read+Or+Die|ROD+Read+Or+Die")]
        public string should_replace_and_include_original_strings_for_dotted_titles(string input)
        {
            Subject.SceneTitles = new List<string> { input };
            return Subject.QueryTitles.Aggregate((a, b) => a + "|" + b);
        }
    }
}
