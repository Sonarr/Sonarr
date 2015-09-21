using NzbDrone.Core.Parser.Model;
using System;

namespace NzbDrone.Core.Parser
{
    public static class SceneChecker
    {
        //This method should prefer false negatives over false positives.
        //It's better not to use a title that might be scene than to use one that isn't scene
        public static bool IsSeriesSceneTitle(string title)
        {
            if (!PassDotsCheck(title)) return false;

            var parsedTitle = Parser.ParseTitle(title);

            return PassParsedInfoCheck(parsedTitle);
        }

        private static bool PassDotsCheck (string title)
        {
            if (!title.Contains(".")) return false;
            if (title.Contains(" ")) return false;
            return true;
        }

        private static bool PassParsedInfoCheck (ParsedInfo parsedTitle)
        {
            if (parsedTitle == null ||
                parsedTitle.ReleaseGroup == null ||
                parsedTitle.Quality.Quality == Qualities.Quality.Unknown ||
                String.IsNullOrWhiteSpace(parsedTitle.Title))
            {
                return false;
            }
            return true;
        }

        //This method should prefer false negatives over false positives.
        //It's better not to use a title that might be scene than to use one that isn't scene
        public static bool IsMovieSceneTitle(string title)
        {
            if (!PassDotsCheck(title)) return false;

            var parsedTitle = Parser.ParseMovieTitle(title);
                        
            return PassParsedInfoCheck (parsedTitle);
        }

    }
}
