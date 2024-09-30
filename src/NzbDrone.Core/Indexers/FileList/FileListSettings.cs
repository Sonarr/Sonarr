using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
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

    public class FileListSettings : PropertywiseEquatable<FileListSettings>, ITorrentIndexerSettings
    {
        private static readonly FileListSettingsValidator Validator = new ();

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
            MultiLanguages = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsPasskey", Privacy = PrivacyLevel.ApiKey)]
        public string Passkey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(3, Label = "IndexerSettingsApiUrl", Advanced = true, HelpText = "IndexerSettingsApiUrlHelpText")]
        public string BaseUrl { get; set; }

        [FieldDefinition(4, Label = "IndexerSettingsCategories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "IndexerSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(5, Label = "IndexerSettingsAnimeCategories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "IndexerSettingsAnimeCategoriesHelpText")]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(8, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum FileListCategories
    {
        [FieldOption(Label = "Anime")]
        Anime = 24,
        [FieldOption(Label = "Animation")]
        Animation = 15,
        [FieldOption(Label = "TV 4K")]
        TV_4K = 27,
        [FieldOption(Label = "TV HD")]
        TV_HD = 21,
        [FieldOption(Label = "TV SD")]
        TV_SD = 23,
        [FieldOption(Label = "Sport")]
        Sport = 13,
        [FieldOption(Label = "RO Dubbed")]
        RoDubbed = 28
    }
}
