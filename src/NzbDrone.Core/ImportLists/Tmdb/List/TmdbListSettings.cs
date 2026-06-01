using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.List;

public class TmdbListSettingsValidator : TmdbSettingsBaseValidator<TmdbListSettings>
{
    public TmdbListSettingsValidator()
    {
        RuleFor(c => c.ListId).Must(id => int.TryParse(id, out var idInt) && idInt > 0)
            .WithMessage($"Must be a valid 32-bit integer greater than zero, and less than or equal to {int.MaxValue}.")
            .When(c => c.ListId.IsNotNullOrWhiteSpace() && c.AccountListId.IsNullOrWhiteSpace());

        RuleFor(c => c).Custom((settings, context) =>
        {
            if (settings.ListId.IsNullOrWhiteSpace() && settings.AccountListId.IsNullOrWhiteSpace())
            {
                context.AddFailure(nameof(settings.ListId), "Must provide a list id, or select an account list.");
                context.AddFailure(nameof(settings.AccountListId), "Must select an account list, or provide a list id.");
            }
            else if (settings.ListId.IsNotNullOrWhiteSpace() && settings.AccountListId.IsNotNullOrWhiteSpace())
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
    public string AccountListId { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsListId", HelpText = "ImportListsTmdbSettingsListIdHelpText")]
    public string ListId { get; set; }
}
