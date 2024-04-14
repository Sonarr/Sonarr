using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.AniList.List
{
    public class AniListSettingsValidator : AniListSettingsBaseValidator<AniListSettings>
    {
        public AniListSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();

            RuleFor(c => c.ImportCurrent).NotEmpty()
                                         .WithMessage("At least one status type must be selected")
                                         .When(c => !(c.ImportPlanning || c.ImportCompleted || c.ImportDropped || c.ImportPaused || c.ImportRepeating));
        }
    }

    public class AniListSettings : AniListSettingsBase<AniListSettings>
    {
        public const string SectionImport = "Import List Status";

        private static readonly AniListSettingsValidator Validator = new ();

        public AniListSettings()
        {
            ImportCurrent = true;
            ImportPlanning = true;
            ImportReleasing = true;
            ImportFinished = true;
        }

        [FieldDefinition(1, Label = "Username", HelpText = "ImportListsAniListSettingsUsernameHelpText")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "ImportListsAniListSettingsImportWatching", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportWatchingHelpText")]
        public bool ImportCurrent { get; set; }

        [FieldDefinition(3, Label = "ImportListsAniListSettingsImportPlanning", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportPlanningHelpText")]
        public bool ImportPlanning { get; set; }

        [FieldDefinition(4, Label = "ImportListsAniListSettingsImportCompleted", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportCompletedHelpText")]
        public bool ImportCompleted { get; set; }

        [FieldDefinition(5, Label = "ImportListsAniListSettingsImportDropped", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportDroppedHelpText")]
        public bool ImportDropped { get; set; }

        [FieldDefinition(6, Label = "ImportListsAniListSettingsImportPaused", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportPausedHelpText")]
        public bool ImportPaused { get; set; }

        [FieldDefinition(7, Label = "ImportListsAniListSettingsImportRepeating", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportRepeatingHelpText")]
        public bool ImportRepeating { get; set; }

        [FieldDefinition(8, Label = "ImportListsAniListSettingsImportFinished", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportFinishedHelpText")]
        public bool ImportFinished { get; set; }

        [FieldDefinition(9, Label = "ImportListsAniListSettingsImportReleasing", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportReleasingHelpText")]
        public bool ImportReleasing { get; set; }

        [FieldDefinition(10, Label = "ImportListsAniListSettingsImportNotYetReleased", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportNotYetReleasedHelpText")]
        public bool ImportUnreleased { get; set; }

        [FieldDefinition(11, Label = "ImportListsAniListSettingsImportCancelled", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportCancelledHelpText")]
        public bool ImportCancelled { get; set; }

        [FieldDefinition(12, Label = "ImportListsAniListSettingsImportHiatus", Type = FieldType.Checkbox, Section = SectionImport, HelpText = "ImportListsAniListSettingsImportHiatusHelpText")]
        public bool ImportHiatus { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
