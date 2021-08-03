using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Framework
{
    public static class NBuilderExtensions
    {
        public static T BuildNew<T>(this ISingleObjectBuilder<T> builder)
            where T : ModelBase, new()
        {
            return builder.With(c => c.Id = 0).Build();
        }

        public static List<T> BuildList<T>(this IListBuilder<T> builder)
            where T : ModelBase, new()
        {
            return builder.Build().ToList();
        }

        public static List<T> BuildListOfNew<T>(this IListBuilder<T> builder)
            where T : ModelBase, new()
        {
            return BuildList<T>(builder.All().With(c => c.Id = 0));
        }
    }
}
