using System;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListSettingsValidator : AbstractValidator<MDBListSettings>
    {
        public MDBListSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.ListUrl)
                .NotEmpty()
                .Must(url => MDBListSettings.TryParseListUrl(url, out _))
                .WithMessage("Must be a valid MDBList list URL");
        }
    }

    public class MDBListSettings : ImportListSettingsBase<MDBListSettings>
    {
        private static readonly MDBListSettingsValidator Validator = new();
        private static readonly Regex ListUrlRegex = new(@"^https?://(?:www\.)?mdblist\.com/lists/(?<username>[^/?#]+)/(?<listname>[^/?#]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string BaseUrl { get; set; } = "https://api.mdblist.com";

        [FieldDefinition(0, Label = "ImportListsMDBListSettingsListUrl", HelpText = "ImportListsMDBListSettingsListUrlHelpText")]
        public string ListUrl { get; set; }

        [FieldDefinition(1, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "ImportListsMDBListSettingsApiKeyHelpText", HelpLink = "https://mdblist.com/preferences/")]
        public string ApiKey { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }

        public static bool TryParseListUrl(string url, out MDBListUrl list)
        {
            list = null;

            if (url.IsNullOrWhiteSpace())
            {
                return false;
            }

            var match = ListUrlRegex.Match(url.Trim());

            if (!match.Success)
            {
                return false;
            }

            list = new MDBListUrl(
                Uri.UnescapeDataString(match.Groups["username"].Value),
                Uri.UnescapeDataString(match.Groups["listname"].Value));

            return true;
        }

        public static MDBListUrl ParseListUrl(string url)
        {
            if (!TryParseListUrl(url, out var list))
            {
                throw new ArgumentException("Invalid MDBList list URL", nameof(url));
            }

            return list;
        }
    }

    public class MDBListUrl
    {
        public MDBListUrl(string username, string listName)
        {
            Username = username;
            ListName = listName;
        }

        public string Username { get; }
        public string ListName { get; }
    }
}
