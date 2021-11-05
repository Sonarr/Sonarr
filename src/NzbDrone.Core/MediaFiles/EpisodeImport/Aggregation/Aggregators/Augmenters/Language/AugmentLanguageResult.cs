using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageResult
    {
        public string Name { get; set; }
        public List<Languages.Language> Languages { get; set; }
        public Confidence Confidence { get; set; }

        public AugmentLanguageResult(List<Languages.Language> languages,
                                    Confidence confidence)
        {
            Languages = languages;
            Confidence = confidence;
        }
    }
}
