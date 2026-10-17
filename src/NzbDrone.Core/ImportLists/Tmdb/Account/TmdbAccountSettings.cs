using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountSettingsValidator : TmdbSettingsBaseValidator<TmdbAccountSettings>
{
    public TmdbAccountSettingsValidator()
    {
        RuleFor(c => c.AccountId).NotEmpty()
            .OverridePropertyName("SignIn")
            .WithMessage("Must authenticate with TMDb");
    }
}

public class TmdbAccountSettings() : TmdbSettingsBase<TmdbAccountSettings>(Validator)
{
    private static readonly TmdbAccountSettingsValidator Validator = new();

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsAccountListType", HelpText = "ImportListsTmdbSettingsAccountListTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbAccountListType))]
    public int AccountListType { get; init; } = (int)TmdbAccountListType.Watchlist;
}
