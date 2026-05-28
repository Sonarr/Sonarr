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

        RuleFor(c => c.AuthToken).NotEmpty()
            .OverridePropertyName("SignIn")
            .WithMessage("Must authenticate with TMDb");
    }
}

public abstract class TmdbSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
    where TSettings : TmdbSettingsBase<TSettings>
{
    private readonly TmdbSettingsBaseValidator<TSettings> _validator;

    protected TmdbSettingsBase(TmdbSettingsBaseValidator<TSettings> validator)
    {
        _validator = validator;

        SignIn = "startOAuth";
    }

    public virtual int MaxPages { get; set; } = 10;

    public override string BaseUrl { get; set; } = "https://api.themoviedb.org";

    [FieldDefinition(97, Label = "Account Id", Type = FieldType.Textbox, Hidden = HiddenType.Hidden, Advanced = true)]
    public string AccountId { get; set; }

    [FieldDefinition(98, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden, Privacy = PrivacyLevel.ApiKey, Advanced = true)]
    public string AuthToken { get; set; }

    [FieldDefinition(99, Label = "ImportListsTmdbSettingsAuthenticateWithTmdb", Type = FieldType.OAuth)]
    public string SignIn { get; set; }

    public override NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(_validator.Validate((TSettings)this));
    }
}
