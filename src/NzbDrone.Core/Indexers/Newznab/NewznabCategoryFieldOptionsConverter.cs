using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public static class NewznabCategoryFieldOptionsConverter
    {
        public static List<FieldSelectOption> GetFieldSelectOptions(List<NewznabCategory> categories)
        {
            // Ignore categories not relevant for Sonarr
            var ignoreCategories = new[] { 0, 1000, 2000, 3000, 4000, 6000, 7000 };

            var result = new List<FieldSelectOption>();

            if (categories == null)
            {
                // Fetching categories failed, use default Newznab categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 5000,
                    Name = "TV",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 5070, Name = "Anime" },
                        new NewznabCategory { Id = 5080, Name = "Documentary" },
                        new NewznabCategory { Id = 5020, Name = "Foreign" },
                        new NewznabCategory { Id = 5040, Name = "HD" },
                        new NewznabCategory { Id = 5045, Name = "UHD" },
                        new NewznabCategory { Id = 5050, Name = "Other" },
                        new NewznabCategory { Id = 5030, Name = "SD" },
                        new NewznabCategory { Id = 5060, Name = "Sport" },
                        new NewznabCategory { Id = 5010, Name = "WEB-DL" }
                    }
                });
            }

            foreach (var category in categories)
            {
                if (ignoreCategories.Contains(category.Id))
                {
                    continue;
                }

                result.Add(new FieldSelectOption
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories)
                    {
                        result.Add(new FieldSelectOption
                        {
                            Value = subcat.Id,
                            Name = subcat.Name,
                            Hint = $"({subcat.Id})",
                            ParentValue = category.Id
                        });
                    }
                }
            }

            result.Sort((l, r) => l.Value.CompareTo(r.Value));

            return result;
        }
    }
}
