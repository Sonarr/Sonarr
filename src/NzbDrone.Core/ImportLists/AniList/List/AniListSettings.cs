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
        public const string unitListStatus = "List Status";
        public const string unitMediaStatus = "Media Status";

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

        [FieldDefinition(0, Label = "Import Watching", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportCurrent { get; set; }

        [FieldDefinition(0, Label = "Import Planning", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportPlanning { get; set; }

        [FieldDefinition(0, Label = "Import Completed", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportCompleted { get; set; }

        [FieldDefinition(0, Label = "Import Dropped", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportDropped { get; set; }

        [FieldDefinition(0, Label = "Import Paused", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportPaused { get; set; }

        [FieldDefinition(0, Label = "Import Repeating", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitListStatus)]
        public bool ImportRepeating { get; set; }

        [FieldDefinition(0, Label = "Import Finished Airing Series", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitMediaStatus)]
        public bool ImportFinished { get; set; }

        [FieldDefinition(0, Label = "Import Currently Airing Series", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitMediaStatus)]
        public bool ImportReleasing { get; set; }

        [FieldDefinition(0, Label = "Import Not Yet Airing Series", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitMediaStatus)]
        public bool ImportUnreleased { get; set; }

        [FieldDefinition(0, Label = "Import Cancelled Series", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitMediaStatus)]
        public bool ImportCancelled { get; set; }

        [FieldDefinition(0, Label = "Import Series on Hiatus", Type = FieldType.Checkbox, Section = sectionImport, Unit = unitMediaStatus)]
        public bool ImportHiatus { get; set; }
    }
}
