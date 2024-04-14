using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalSettingsValidator : AbstractValidator<MyAnimeListSettings>
    {
        public MalSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with MyAnimeList");

            RuleFor(c => c.ListStatus).Custom((status, context) =>
            {
                if (!Enum.IsDefined(typeof(MyAnimeListStatus), status))
                {
                    context.AddFailure($"Invalid status: {status}");
                }
            });
        }
    }

    public class MyAnimeListSettings : ImportListSettingsBase<MyAnimeListSettings>
    {
        private static readonly MalSettingsValidator Validator = new ();

        public override string BaseUrl { get; set; }  = "https://api.myanimelist.net/v2";

        [FieldDefinition(0, Label = "ImportListsMyAnimeListSettingsListStatus", Type = FieldType.Select, SelectOptions = typeof(MyAnimeListStatus), HelpText = "ImportListsMyAnimeListSettingsListStatusHelpText")]
        public int ListStatus { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(99, Label = "ImportListsMyAnimeListSettingsAuthenticateWithMyAnimeList", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
