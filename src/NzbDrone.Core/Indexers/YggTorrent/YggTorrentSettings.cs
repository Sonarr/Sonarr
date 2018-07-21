using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.YggTorrent
{
    public class YggTorrentSettingsValidator : AbstractValidator<YggTorrentSettings>
    {
        public YggTorrentSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.User).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.DownloadUrlFormat).NotEmpty();
            RuleFor(c => c.SearchUrlFormat).NotEmpty();
            RuleFor(c => c.XPathItems).NotEmpty();
            RuleFor(c => c.XPathItem).NotEmpty().Must(p => p.Split(';').Length == 6).WithMessage("Must have 6 falue with ';' separator."); 

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class YggTorrentSettings : ITorrentIndexerSettings
    {
        private static readonly YggTorrentSettingsValidator Validator = new YggTorrentSettingsValidator();
        private string _baseUrl;

        public YggTorrentSettings()
        {
            BaseUrl = "https://ww1.yggtorrent.is";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            XPathItems = "//section[@id=\"#torrents\"]/*/table[@class=\"table\"]/tbody/tr";
            XPathItem = "td[3]/a|target;td[2]/a;td[5]/div;td[6];td[8];td[9]";
            DownloadUrlFormat = "engine/download_torrent?id=";
            SearchUrlFormat = "engine/search?do=search&name=";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = value.Trim().TrimEnd('/');
        }

        [FieldDefinition(1, Label = "Login")]
        public string User { get; set; }

        [FieldDefinition(2, Label = "Password")]
        public string Password { get; set; }

        [FieldDefinition(3, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(4)]
        public SeedCriteriaSettings SeedCriteria { get; } = new SeedCriteriaSettings();

        [FieldDefinition(5, Advanced = true, Label = "XPath Items", HelpText = "XPath for release group items to search in HTML page.")]
        public string XPathItems { get; set; }

        [FieldDefinition(6, Advanced = true, Label = "XPath Item", HelpText = "XPath (relative from XPathItems) to extract. Match to torrentId;name;timestamp;size;seeders;leechers. " +
                                                                              "Foreach one the InnerHTML will be got, except if '|attribute_name' is used " +
                                                                              "then the attribute value will be get instead.")]
        public string XPathItem { get; set; }


        [FieldDefinition(7, Advanced = true, Label = "Relative download URL format")]
        public string DownloadUrlFormat { get; set; }

        [FieldDefinition(8, Advanced = true, Label = "Relative search URL format")]
        public string SearchUrlFormat { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
