using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.AniList.List
{
    public class AniListSettingsValidator : AniListSettingsBaseValidator<AniListSettings>
    {
        public AniListSettingsValidator()
        : base()
        {
            RuleFor(c => c.Username).NotEmpty();

            RuleFor(c => c.ImportCurrent).NotEmpty()
                                         .WithMessage("At least one status type must be selected")
                                         .When(c => !(c.ImportPlanning || c.ImportCompleted || c.ImportDropped || c.ImportPaused || c.ImportRepeating));
        }
    }

    public class AniListSettings : AniListSettingsBase<AniListSettings>
    {
        public const string sectionImport = "Import List Status";

        public AniListSettings()
        : base()
        {
            ImportCurrent = true;
            ImportPlanning = true;
            ImportReleasing = true;
            ImportFinished = true;
        }

        protected override AbstractValidator<AniListSettings> Validator => new AniListSettingsValidator();

        [FieldDefinition(1, Label = "Username", HelpText = "Username for the List to import from")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Import Watching", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: Currently Watching")]
        public bool ImportCurrent { get; set; }

        [FieldDefinition(3, Label = "Import Planning", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: Planning to Watch")]
        public bool ImportPlanning { get; set; }

        [FieldDefinition(4, Label = "Import Completed", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: Completed Watching")]
        public bool ImportCompleted { get; set; }

        [FieldDefinition(5, Label = "Import Dropped", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: Dropped")]
        public bool ImportDropped { get; set; }

        [FieldDefinition(6, Label = "Import Paused", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: On Hold")]
        public bool ImportPaused { get; set; }

        [FieldDefinition(7, Label = "Import Repeating", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "List: Currently Rewatching")]
        public bool ImportRepeating { get; set; }

        [FieldDefinition(8, Label = "Import Finished", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "Media: All episodes have aired")]
        public bool ImportFinished { get; set; }

        [FieldDefinition(9, Label = "Import Releasing", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "Media: Currently airing new episodes")]
        public bool ImportReleasing { get; set; }

        [FieldDefinition(10, Label = "Import Not Yet Released", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "Media: Airing has not yet started")]
        public bool ImportUnreleased { get; set; }

        [FieldDefinition(11, Label = "Import Cancelled", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "Media: Series is cancelled")]
        public bool ImportCancelled { get; set; }

        [FieldDefinition(12, Label = "Import Hiatus", Type = FieldType.Checkbox, Section = sectionImport, HelpText = "Media: Series on Hiatus")]
        public bool ImportHiatus { get; set; }
    }
}
