using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalSettingsValidator : AbstractValidator<MalListSettings>
    {
        public MalSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ClientId).NotEmpty()
                           .OverridePropertyName("ClientId")
                           .WithMessage("Must input Client ID from MAL, created in the API section of your account");
            RuleFor(c => c.ClientSecret).NotEmpty()
                           .OverridePropertyName("ClientSecret")
                           .WithMessage("Must input Client Secret from MAL, created in the API section of your account");
            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with MyAnimeList");
        }
    }

    public class MalListSettings : IImportListSettings
    {
        public string BaseUrl { get; set; }

        protected AbstractValidator<MalListSettings> Validator => new MalSettingsValidator();

        // This constructor is called when we try to add a new list
        public MalListSettings()
        {
            BaseUrl = "https://api.myanimelist.net/v2";
            ListStatus = MalAnimeStatus.Watching;
        }

        [FieldDefinition(0, Label = "List Status", Type = FieldType.Select, SelectOptions = typeof(MalAnimeStatus), HelpText = "Type of list status you're seeking to import from")]
        public MalAnimeStatus ListStatus { get; set; }

        [FieldDefinition(0, Label = "Client ID", Type = FieldType.Textbox)]
        public string ClientId { get; set; }

        [FieldDefinition(0, Label = "Client Secret", Type = FieldType.Textbox)]
        public string ClientSecret { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Visible)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Visible)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Visible)]
        public DateTime Expires { get; set; }

        [FieldDefinition(99, Label = "Authenticate With MyAnimeList", Type = FieldType.OAuth, HelpTextWarning = "Client ID and Client Secret must be filled out before authenticating with MAL")]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
