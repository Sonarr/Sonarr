using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListSettingsValidator : TmdbSettingsBaseValidator<TmdbListSettings>
{
    public TmdbListSettingsValidator()
    {
        RuleFor(c => c.ListId)
            .GreaterThan(0)
            .When(c => c.ListId.HasValue && !c.AccountListId.HasValue);

        RuleFor(c => c).Custom((settings, context) =>
        {
            if (!settings.ListId.HasValue && !settings.AccountListId.HasValue)
            {
                context.AddFailure(nameof(settings.ListId), "Must provide a list id, or select an account list.");
                context.AddFailure(nameof(settings.AccountListId), "Must select an account list, or provide a list id.");
            }
            else if (settings.ListId.HasValue && settings.AccountListId.HasValue)
            {
                context.AddFailure(nameof(settings.ListId), "Account list has already been selected.");
                context.AddFailure(nameof(settings.AccountListId), "List id has already been provided.");
            }
        });
    }
}

public class TmdbListSettings : TmdbSettingsBase<TmdbListSettings>
{
    private static readonly TmdbListSettingsValidator Validator = new();

    public TmdbListSettings()
        : base(Validator)
    {
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsAccountListId", HelpText = "ImportListsTmdbSettingsAccountListIdHelpText", Type = FieldType.Select, SelectOptionsProviderAction = "getAccountLists", Advanced = true)]
    public int? AccountListId { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsListId", HelpText = "ImportListsTmdbSettingsListIdHelpText")]
    public int? ListId { get; set; }
}
