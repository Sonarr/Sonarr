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

            RuleFor(c => c.ListId).Must(id => MDBListSettings.GetListPath(id) != null)
                                  .WithMessage("Must be an MDBList list URL");

            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with MDBList");
        }
    }

    public class MDBListSettings : ImportListSettingsBase<MDBListSettings>
    {
        private static readonly MDBListSettingsValidator Validator = new();

        private static readonly Regex ListUrlRegex = new(@"(?:^|/)lists/(?<user>[^/?#\s]+)/(?<slug>[^/?#\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MDBListSettings()
        {
            SignIn = "startOAuth";
        }

        public override string BaseUrl { get; set; } = "https://api.mdblist.com";

        [FieldDefinition(0, Label = "ImportListsMDBListSettingsList", HelpText = "ImportListsMDBListSettingsListHelpText", Placeholder = "https://mdblist.com/lists/username/list-name")]
        public string ListId { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(98, Label = "ImportListsMDBListSettingsAuthUser", HelpText = "ImportListsMDBListSettingsAuthUserHelpText", Type = FieldType.Textbox, Hidden = HiddenType.HiddenIfNotSet)]
        public string AuthUser { get; set; }

        [FieldDefinition(99, Label = "ImportListsMDBListSettingsAuthenticateWithMDBList", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public static string GetListPath(string list)
        {
            if (list.IsNullOrWhiteSpace())
            {
                return null;
            }

            var match = ListUrlRegex.Match(list);

            if (!match.Success)
            {
                return null;
            }

            return $"lists/{Uri.EscapeDataString(Uri.UnescapeDataString(match.Groups["user"].Value))}/{Uri.EscapeDataString(Uri.UnescapeDataString(match.Groups["slug"].Value))}";
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
