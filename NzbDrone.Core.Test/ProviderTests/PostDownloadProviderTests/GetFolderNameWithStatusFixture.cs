

using System.Linq;
using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    
    public class GetFolderNameWithStatusFixture : CoreTest
    {
        [TestCase(@"c:\_NzbDrone_InvalidEpisode_Title", @"c:\_UnknownSeries_Title", PostDownloadStatusType.UnknownSeries)]
        [TestCase(@"c:\Title", @"c:\_Failed_Title", PostDownloadStatusType.Failed)]
        [TestCase(@"c:\Root\Test Title", @"c:\Root\_ParseError_Test Title", PostDownloadStatusType.ParseError)]
        public void GetFolderNameWithStatus_should_return_a_string_with_the_error_removing_existing_error(string currentName, string excpectedName, PostDownloadStatusType status)
        {
            PostDownloadProvider.GetTaggedFolderName(new DirectoryInfo(currentName), status).Should().Be(
                excpectedName);
        }

        [TestCase(PostDownloadStatusType.NoError)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetFolderNameWithStatus_should_throw_if_status_is_not_an_error(PostDownloadStatusType status)
        {
            PostDownloadProvider.GetTaggedFolderName(new DirectoryInfo(TempFolder), status);
        }


        [TestCase("_NzbDrone_ParseError_The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("_Status_The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("The Office (US) - S01E01 - Episode Title", "The Office (US) - S01E01 - Episode Title")]
        [TestCase("_The Office (US) - S01E01 - Episode Title", "_The Office (US) - S01E01 - Episode Title")]
        public void RemoveStatus_should_remove_status_string_from_folder_name(string folderName, string cleanFolderName)
        {
            PostDownloadProvider.RemoveStatusFromFolderName(folderName).Should().Be(cleanFolderName);
        }
    }
}