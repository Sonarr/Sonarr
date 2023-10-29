using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsSettingsValidator : AbstractValidator<HDBitsSettings>
    {
        public HDBitsSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class HDBitsSettings : ITorrentIndexerSettings
    {
        private static readonly HDBitsSettingsValidator Validator = new ();

        public HDBitsSettings()
        {
            BaseUrl = "https://hdbits.org";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new[] { (int)HdBitsCategory.Tv, (int)HdBitsCategory.Documentary };
            Codecs = Array.Empty<int>();
            Mediums = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "IndexerSettingsApiUrl", Advanced = true, HelpText = "IndexerSettingsApiUrlHelpText")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "IndexerHDBitsSettingsCategories", Type = FieldType.Select, SelectOptions = typeof(HdBitsCategory), HelpText = "IndexerHDBitsSettingsCategoriesHelpText")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Label = "IndexerHDBitsSettingsCodecs", Type = FieldType.Select, SelectOptions = typeof(HdBitsCodec), Advanced = true, HelpText = "IndexerHDBitsSettingsCodecsHelpText")]
        public IEnumerable<int> Codecs { get; set; }

        [FieldDefinition(5, Label = "IndexerHDBitsSettingsMediums", Type = FieldType.Select, SelectOptions = typeof(HdBitsMedium), Advanced = true, HelpText = "IndexerHDBitsSettingsMediumsHelpText")]
        public IEnumerable<int> Mediums { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(8, Type = FieldType.Checkbox, Label = "Reject Blocklisted Torrent Hashes While Grabbing", HelpText = "If a torrent is blocked by hash it may not properly be rejected during RSS/Search for some indexers, enabling this will allow it to be rejected after the torrent is grabbed, but before it is sent to the client.", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum HdBitsCategory
    {
        [FieldOption(label: "Movie")]
        Movie = 1,
        [FieldOption(label: "TV")]
        Tv = 2,
        [FieldOption(label: "Documentary")]
        Documentary = 3,
        [FieldOption(label: "Music")]
        Music = 4,
        [FieldOption(label: "Sport")]
        Sport = 5,
        [FieldOption(label: "Audio Track")]
        Audio = 6,
        [FieldOption(label: "XXX")]
        Xxx = 7,
        [FieldOption(label: "Misc/Demo")]
        MiscDemo = 8
    }

    public enum HdBitsCodec
    {
        [FieldOption(label: "H.264")]
        H264 = 1,
        [FieldOption(label: "MPEG-2")]
        Mpeg2 = 2,
        [FieldOption(label: "VC-1")]
        Vc1 = 3,
        [FieldOption(label: "XviD")]
        Xvid = 4,
        [FieldOption(label: "HEVC")]
        Hevc = 5
    }

    public enum HdBitsMedium
    {
        [FieldOption(label: "Blu-ray/HD DVD")]
        Bluray = 1,
        [FieldOption(label: "Encode")]
        Encode = 3,
        [FieldOption(label: "Capture")]
        Capture = 4,
        [FieldOption(label: "Remux")]
        Remux = 5,
        [FieldOption(label: "WEB-DL")]
        WebDl = 6
    }
}
