using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport;

public interface ILocalEpisodeCustomFormatCalculationService
{
    public List<CustomFormat> ParseEpisodeCustomFormats(LocalEpisode localEpisode);
    public void UpdateEpisodeCustomFormats(LocalEpisode localEpisode);
}

public class LocalEpisodeCustomFormatCalculationService : ILocalEpisodeCustomFormatCalculationService
{
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public LocalEpisodeCustomFormatCalculationService(IBuildFileNames fileNameBuilder, ICustomFormatCalculationService formatCalculator)
    {
        _fileNameBuilder = fileNameBuilder;
        _formatCalculator = formatCalculator;
    }

    public List<CustomFormat> ParseEpisodeCustomFormats(LocalEpisode localEpisode)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localEpisode.Episodes, localEpisode.Series, localEpisode.ToEpisodeFile());
        return _formatCalculator.ParseCustomFormat(localEpisode, fileNameUsedForCustomFormatCalculation);
    }

    public void UpdateEpisodeCustomFormats(LocalEpisode localEpisode)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localEpisode.Episodes, localEpisode.Series, localEpisode.ToEpisodeFile());
        localEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(localEpisode, fileNameUsedForCustomFormatCalculation);
        localEpisode.FileNameUsedForCustomFormatCalculation = fileNameUsedForCustomFormatCalculation;
        localEpisode.CustomFormatScore = localEpisode.Series.QualityProfile?.Value.CalculateCustomFormatScore(localEpisode.CustomFormats) ?? 0;

        localEpisode.OriginalFileNameCustomFormats = _formatCalculator.ParseCustomFormat(localEpisode, Path.GetFileName(localEpisode.Path));
        localEpisode.OriginalFileNameCustomFormatScore = localEpisode.Series.QualityProfile?.Value.CalculateCustomFormatScore(localEpisode.OriginalFileNameCustomFormats) ?? 0;
    }
}
