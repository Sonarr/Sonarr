using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubSettingsValidator : AbstractValidator<FanzubSettings>
    {
        public FanzubSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.SearchPriority).InclusiveBetween(0, 100);
        }
    }

    public class FanzubSettings : IIndexerSettings
    {
        private static readonly FanzubSettingsValidator Validator = new FanzubSettingsValidator();

        public FanzubSettings()
        {
            BaseUrl = "http://fanzub.com/rss/";
            SearchPriority = IndexerDefaults.SEARCH_PRIORITY;
        }

        [FieldDefinition(0, Label = "Rss URL", HelpText = "Enter to URL to an Fanzub compatible RSS feed")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Type = FieldType.Number, Label = "Search Priority", HelpText = "Search Priority from 0 (Highest) to 100 (Lowest). Default: 100.")]
        public int SearchPriority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
