using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Helpers
{
    public static class EpisodeRenameHelper
    {
        public static string GetNewName(EpisodeRenameModel erm)
        {
            //Todo: Get the users preferred naming convention instead of hard-coding it

            if (erm.EpisodeFile.Episodes.Count == 1)
            {
                return String.Format("{0} - S{1:00}E{2:00} - {3}", erm.SeriesName,
                                     erm.EpisodeFile.Episodes[0].SeasonNumber, erm.EpisodeFile.Episodes[0].EpisodeNumber,
                                     erm.EpisodeFile.Episodes[0].Title);
            }

            var epNumberString = String.Empty;
            var epNameString = String.Empty;

            foreach (var episode in erm.EpisodeFile.Episodes)
            {
                epNumberString = epNumberString + String.Format("E{0:00}", episode.EpisodeNumber);
                epNameString = epNameString + String.Format("+ {0}", episode.Title).Trim(' ', '+');
            }

            return String.Format("{0} - S{1:00}E{2} - {3}", erm.SeriesName, erm.EpisodeFile.Episodes[0].SeasonNumber,
                                 epNumberString, epNameString);
        }

        public static string GetSeasonFolder(int seasonNumber, string seasonFolderFormat)
        {
            return seasonFolderFormat.Replace("%s", seasonNumber.ToString()).Replace("%0s", seasonNumber.ToString("00"));
        }

        public static string GetNameForNotify(EpisodeRenameModel erm)
        {
            if (erm.EpisodeFile.Episodes.Count == 1)
            {
                return String.Format("{0} - S{1:00}E{2:00} - {3}", erm.SeriesName,
                                     erm.EpisodeFile.Episodes[0].SeasonNumber, erm.EpisodeFile.Episodes[0].EpisodeNumber,
                                     erm.EpisodeFile.Episodes[0].Title);
            }

            var epNumberString = String.Empty;
            var epNameString = String.Empty;

            foreach (var episode in erm.EpisodeFile.Episodes)
            {
                epNumberString = epNumberString + String.Format("E{0:00}", episode.EpisodeNumber);
                epNameString = epNameString + String.Format("+ {0}", episode.Title).Trim(' ', '+');
            }

            return String.Format("{0} - S{1:00}E{2} - {3}", erm.SeriesName, erm.EpisodeFile.Episodes[0].SeasonNumber,
                                 epNumberString, epNameString);
        }

        public static string CleanFilename(string name)
        {
	        string result = name;
	        string[] badCharacters = {"\\", "/", "<", ">", "?", "*", ":", "|", "\""};
	        string[] goodCharacters = {"+", "+", "{", "}", "!", "@", "-", "#", "`"};

	        for (int i = 0; i < badCharacters.Length; i++)
		        result = result.Replace(badCharacters[i], goodCharacters[i]);

	        return result.Trim();
        }
    }
}