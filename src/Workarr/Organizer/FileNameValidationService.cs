using FluentValidation.Results;
using Workarr.Parser.Model;
using Workarr.Tv;

namespace Workarr.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateStandardFilename(SampleResult sampleResult);
        ValidationFailure ValidateDailyFilename(SampleResult sampleResult);
        ValidationFailure ValidateAnimeFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateStandardFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardEpisodeFormat", ERROR_MESSAGE);
            var parsedEpisodeInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedEpisodeInfo == null)
            {
                return validationFailure;
            }

            if (!ValidateSeasonAndEpisodeNumbers(sampleResult.Episodes, parsedEpisodeInfo))
            {
                return validationFailure;
            }

            return null;
        }

        public ValidationFailure ValidateDailyFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("DailyEpisodeFormat", ERROR_MESSAGE);
            var parsedEpisodeInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedEpisodeInfo == null)
            {
                return validationFailure;
            }

            if (parsedEpisodeInfo.IsDaily)
            {
                if (!parsedEpisodeInfo.AirDate.Equals(sampleResult.Episodes.Single().AirDate))
                {
                    return validationFailure;
                }

                return null;
            }

            if (!ValidateSeasonAndEpisodeNumbers(sampleResult.Episodes, parsedEpisodeInfo))
            {
                return validationFailure;
            }

            return null;
        }

        public ValidationFailure ValidateAnimeFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("AnimeEpisodeFormat", ERROR_MESSAGE);
            var parsedEpisodeInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedEpisodeInfo == null)
            {
                return validationFailure;
            }

            if (parsedEpisodeInfo.AbsoluteEpisodeNumbers.Any())
            {
                if (!parsedEpisodeInfo.AbsoluteEpisodeNumbers.First().Equals(sampleResult.Episodes.First().AbsoluteEpisodeNumber))
                {
                    return validationFailure;
                }

                return null;
            }

            if (!ValidateSeasonAndEpisodeNumbers(sampleResult.Episodes, parsedEpisodeInfo))
            {
                return validationFailure;
            }

            return null;
        }

        private bool ValidateSeasonAndEpisodeNumbers(List<Episode> episodes, ParsedEpisodeInfo parsedEpisodeInfo)
        {
            if (parsedEpisodeInfo.SeasonNumber != episodes.First().SeasonNumber ||
                !parsedEpisodeInfo.EpisodeNumbers.OrderBy(e => e).SequenceEqual(episodes.Select(e => e.EpisodeNumber).OrderBy(e => e)))
            {
                return false;
            }

            return true;
        }
    }
}
