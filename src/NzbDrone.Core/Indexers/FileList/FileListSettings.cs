using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListSettingsValidator : AbstractValidator<FileListSettings>
    {
        public FileListSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Passkey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class FileListSettings : ITorrentIndexerSettings
    {
        private static readonly FileListSettingsValidator Validator = new FileListSettingsValidator();

        public FileListSettings()
        {
            BaseUrl = "https://filelist.io";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new int[]
            {
                (int)FileListCategories.TV_SD,
                (int)FileListCategories.TV_HD,
                (int)FileListCategories.TV_4K
            };

            AnimeCategories = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsPasskey", Privacy = PrivacyLevel.ApiKey)]
        public string Passkey { get; set; }

        [FieldDefinition(3, Label = "IndexerSettingsApiUrl", Advanced = true, HelpText = "IndexerSettingsApiUrlHelpText")]
        public string BaseUrl { get; set; }

        [FieldDefinition(4, Label = "IndexerSettingsCategories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "IndexerSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(5, Label = "IndexerSettingsAnimeCategories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "IndexerSettingsAnimeCategoriesHelpText")]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new SeedCriteriaSettings();

        [FieldDefinition(8, Type = FieldType.Checkbox, Label = "Reject Blocklisted Torrent Hashes While Grabbing", HelpText = "If a torrent is blocked by hash it may not properly be rejected during RSS/Search for some indexers, enabling this will allow it to be rejected after the torrent is grabbed, but before it is sent to the client.", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum FileListCategories
    {
        [FieldOption]
        Anime = 24,
        [FieldOption]
        Animation = 15,
        [FieldOption]
        TV_4K = 27,
        [FieldOption]
        TV_HD = 21,
        [FieldOption]
        TV_SD = 23,
        [FieldOption]
        Sport = 13
    }
}
