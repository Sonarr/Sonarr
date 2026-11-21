using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListSettingsValidator : AbstractValidator<MDBListSettings>
    {
        public MDBListSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.ListId).Must(id => MDBListHelper.GetListPath(id) != null)
                                  .WithMessage("Must be an MDBList list URL");

            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with MDBList");
        }
    }

    public class MDBListSettings : ImportListSettingsBase<MDBListSettings>
    {
        private static readonly MDBListSettingsValidator Validator = new();

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

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
