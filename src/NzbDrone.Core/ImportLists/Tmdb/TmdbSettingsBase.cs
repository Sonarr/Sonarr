using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Tmdb;

public abstract class TmdbSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : TmdbSettingsBase<TSettings>
{
    protected TmdbSettingsBaseValidator()
    {
        RuleFor(c => c.BaseUrl).ValidRootUrl();

        RuleFor(c => c.BaseUrl).Must(url => string.Equals(url?.TrimEnd('/'), TmdbSettingsBase<TSettings>.DefaultBaseUrl, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Must be " + TmdbSettingsBase<TSettings>.DefaultBaseUrl);

        RuleFor(c => c.AuthToken).NotEmpty()
            .OverridePropertyName("SignIn")
            .WithMessage("Must authenticate with TMDb");
    }
}

public abstract class TmdbSettingsBase<TSettings>(TmdbSettingsBaseValidator<TSettings> validator) : ImportListSettingsBase<TSettings>
    where TSettings : TmdbSettingsBase<TSettings>
{
    public const string DefaultBaseUrl = "https://api.themoviedb.org";

    public override string BaseUrl { get; set; } = DefaultBaseUrl;

    [FieldDefinition(0, Label = "ImportListsTmdbSettingsAccountId", Type = FieldType.Textbox, Hidden = HiddenType.Hidden, Advanced = true)]
    public string AccountId { get; init; }

    [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden, Privacy = PrivacyLevel.ApiKey, Advanced = true)]
    public string AuthToken { get; set; }

    [FieldDefinition(99, Label = "ImportListsTmdbSettingsAuthenticateWithTmdb", Type = FieldType.OAuth)]
    public string SignIn { get; set; } = "startOAuth";

    public override NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(validator.Validate((TSettings)this));
    }
}
