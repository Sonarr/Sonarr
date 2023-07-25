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
        }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox)]
        public DateTime Expires { get; set; }

        [FieldDefinition(0, Label = "Auth User", Type = FieldType.Textbox)]
        public string AuthUser { get; set; }

        [FieldDefinition(99, Label = "Auth With MAL", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
