using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public static class QualityItemsValidator
    {
        public static IRuleBuilderOptions<T, IList<QualityProfileQualityItemResource>> ValidItems<T>(this IRuleBuilder<T, IList<QualityProfileQualityItemResource>> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new AllowedValidator<T>());
            ruleBuilder.SetValidator(new QualityNameValidator<T>());
            ruleBuilder.SetValidator(new GroupItemValidator<T>());
            ruleBuilder.SetValidator(new ItemGroupIdValidator<T>());
            ruleBuilder.SetValidator(new UniqueIdValidator<T>());
            ruleBuilder.SetValidator(new UniqueQualityIdValidator<T>());
            return ruleBuilder.SetValidator(new ItemGroupNameValidator<T>());
        }
    }

    public class AllowedValidator<T> : PropertyValidator
    {
        public AllowedValidator()
            : base("Must contain at least one allowed quality")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (list == null)
            {
                return false;
            }

            if (!list.Any(c => c.Allowed))
            {
                return false;
            }

            return true;
        }
    }

    public class GroupItemValidator<T> : PropertyValidator
    {
        public GroupItemValidator()
            : base("Groups must contain multiple qualities")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (items.Any(i => i.Name.IsNotNullOrWhiteSpace() && i.Items.Count <= 1))
            {
                return false;
            }

            return true;
        }
    }

    public class QualityNameValidator<T> : PropertyValidator
    {
        public QualityNameValidator()
            : base("Individual qualities should not be named")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (items.Any(i => i.Name.IsNotNullOrWhiteSpace() && i.Quality != null))
            {
                return false;
            }

            return true;
        }
    }

    public class ItemGroupNameValidator<T> : PropertyValidator
    {
        public ItemGroupNameValidator()
            : base("Groups must have a name")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (items.Any(i => i.Quality == null && i.Name.IsNullOrWhiteSpace()))
            {
                return false;
            }

            return true;
        }
    }

    public class ItemGroupIdValidator<T> : PropertyValidator
    {
        public ItemGroupIdValidator()
            : base("Groups must have an ID")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (items.Any(i => i.Quality == null && i.Id == 0))
            {
                return false;
            }

            return true;
        }
    }

    public class UniqueIdValidator<T> : PropertyValidator
    {
        public UniqueIdValidator()
            : base("Groups must have a unique ID")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;

            if (items.Where(i => i.Id > 0).Select(i => i.Id).GroupBy(i => i).Any(g => g.Count() > 1))
            {
                return false;
            }

            return true;
        }
    }

    public class UniqueQualityIdValidator<T> : PropertyValidator
    {
        public UniqueQualityIdValidator()
            : base("Qualities can only be used once")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var items = context.PropertyValue as IList<QualityProfileQualityItemResource>;
            var qualityIds = new HashSet<int>();

            foreach (var item in items)
            {
                if (item.Id > 0)
                {
                    foreach (var quality in item.Items)
                    {
                        if (qualityIds.Contains(quality.Quality.Id))
                        {
                            return false;
                        }

                        qualityIds.Add(quality.Quality.Id);
                    }
                }
                else
                {
                    if (qualityIds.Contains(item.Quality.Id))
                    {
                        return false;
                    }

                    qualityIds.Add(item.Quality.Id);
                }
            }

            return true;
        }
    }
}
