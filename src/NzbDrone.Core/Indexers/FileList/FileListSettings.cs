using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
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

            AnimeCategories = new int[0];
        }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "Passkey", Privacy = PrivacyLevel.ApiKey)]
        public string Passkey { get; set; }

        [FieldDefinition(3, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(4, Label = "Categories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "Categories for use in search and feeds, leave blank to disable standard/daily shows")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(5, Label = "Anime Categories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "Categories for use in search and feeds, leave blank to disable anime")]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new SeedCriteriaSettings();

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
