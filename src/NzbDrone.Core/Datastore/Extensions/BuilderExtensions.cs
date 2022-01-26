using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore
{
    public static class SqlBuilderExtensions
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(SqlBuilderExtensions));

        public static bool LogSql { get; set; }

        public static SqlBuilder Select(this SqlBuilder builder, params Type[] types)
        {
            return builder.Select(types.Select(x => $"\"{TableMapping.Mapper.TableNameMapping(x)}\".*").Join(", "));
        }

        public static SqlBuilder SelectDistinct(this SqlBuilder builder, params Type[] types)
        {
            return builder.Select("DISTINCT " + types.Select(x => $"\"{TableMapping.Mapper.TableNameMapping(x)}\".*").Join(", "));
        }

        public static SqlBuilder SelectCount(this SqlBuilder builder)
        {
            return builder.Select("COUNT(*)");
        }

        public static SqlBuilder SelectCountDistinct<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property)
        {
            var table = TableMapping.Mapper.TableNameMapping(typeof(TModel));
            var propName = property.GetMemberName().Name;
            return builder.Select($"COUNT(DISTINCT \"{table}\".\"{propName}\")");
        }

        public static SqlBuilder Where<TModel>(this SqlBuilder builder, Expression<Func<TModel, bool>> filter)
        {
            var wb = GetWhereBuilder(builder.DatabaseType, filter, true, builder.Sequence);

            return builder.Where(wb.ToString(), wb.Parameters);
        }

        public static SqlBuilder WherePostgres<TModel>(this SqlBuilder builder, Expression<Func<TModel, bool>> filter)
        {
            var wb = new WhereBuilderPostgres(filter, true, builder.Sequence);

            return builder.Where(wb.ToString(), wb.Parameters);
        }

        public static SqlBuilder OrWhere<TModel>(this SqlBuilder builder, Expression<Func<TModel, bool>> filter)
        {
            var wb = GetWhereBuilder(builder.DatabaseType, filter, true, builder.Sequence);

            return builder.OrWhere(wb.ToString(), wb.Parameters);
        }

        public static SqlBuilder Join<TLeft, TRight>(this SqlBuilder builder, Expression<Func<TLeft, TRight, bool>> filter)
        {
            var wb = GetWhereBuilder(builder.DatabaseType, filter, false, builder.Sequence);

            var rightTable = TableMapping.Mapper.TableNameMapping(typeof(TRight));

            return builder.Join($"\"{rightTable}\" ON {wb.ToString()}");
        }

        public static SqlBuilder LeftJoin<TLeft, TRight>(this SqlBuilder builder, Expression<Func<TLeft, TRight, bool>> filter)
        {
            var wb = GetWhereBuilder(builder.DatabaseType, filter, false, builder.Sequence);

            var rightTable = TableMapping.Mapper.TableNameMapping(typeof(TRight));

            return builder.LeftJoin($"\"{rightTable}\" ON {wb.ToString()}");
        }

        public static SqlBuilder GroupBy<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property)
        {
            var table = TableMapping.Mapper.TableNameMapping(typeof(TModel));
            var propName = property.GetMemberName().Name;
            return builder.GroupBy($"\"{table}\".\"{propName}\"");
        }

        public static SqlBuilder.Template AddSelectTemplate(this SqlBuilder builder, Type type)
        {
            return builder.AddTemplate(TableMapping.Mapper.SelectTemplate(type)).LogQuery();
        }

        public static SqlBuilder.Template AddPageCountTemplate(this SqlBuilder builder, Type type)
        {
            return builder.AddTemplate(TableMapping.Mapper.PageCountTemplate(type)).LogQuery();
        }

        public static SqlBuilder.Template AddDeleteTemplate(this SqlBuilder builder, Type type)
        {
            return builder.AddTemplate(TableMapping.Mapper.DeleteTemplate(type)).LogQuery();
        }

        public static SqlBuilder.Template LogQuery(this SqlBuilder.Template template)
        {
            if (LogSql)
            {
                Logger.Trace(GetSqlLogString(template.RawSql, template.Parameters));
            }

            return template;
        }

        public static void LogQuery(string sql, object parameters)
        {
            if (LogSql)
            {
                Logger.Trace(GetSqlLogString(sql, parameters));
            }
        }

        public static string GetSqlLogString(string sql, object paramsObject)
        {
            var parameters = new DynamicParameters(paramsObject);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("==== Begin Query Trace ====");
            sb.AppendLine();
            sb.AppendLine("QUERY TEXT:");
            sb.AppendLine(sql);
            sb.AppendLine();
            sb.AppendLine("PARAMETERS:");

            foreach (var p in parameters.ToDictionary())
            {
                var val = (p.Value is string) ? string.Format("\"{0}\"", p.Value) : p.Value;
                sb.AppendFormat("{0} = [{1}]", p.Key, val.ToJson() ?? "NULL").AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("==== End Query Trace ====");
            sb.AppendLine();

            return sb.ToString();
        }

        private static WhereBuilder GetWhereBuilder(DatabaseType databaseType, Expression filter, bool requireConcrete, int seq)
        {
            if (databaseType == DatabaseType.PostgreSQL)
            {
                return new WhereBuilderPostgres(filter, requireConcrete, seq);
            }
            else
            {
                return new WhereBuilderSqlite(filter, requireConcrete, seq);
            }
        }

        private static Dictionary<string, object> ToDictionary(this DynamicParameters dynamicParams)
        {
            var argsDictionary = new Dictionary<string, object>();
            var iLookup = (SqlMapper.IParameterLookup)dynamicParams;

            foreach (var paramName in dynamicParams.ParameterNames)
            {
                var value = iLookup[paramName];
                argsDictionary.Add(paramName, value);
            }

            var templates = dynamicParams.GetType().GetField("templates", BindingFlags.NonPublic | BindingFlags.Instance);
            if (templates != null)
            {
                if (templates.GetValue(dynamicParams) is List<object> list)
                {
                    foreach (var objProps in list.Select(obj => obj.GetPropertyValuePairs().ToList()))
                    {
                        objProps.ForEach(p => argsDictionary.Add(p.Key, p.Value));
                    }
                }
            }

            return argsDictionary;
        }

        private static Dictionary<string, object> GetPropertyValuePairs(this object obj)
        {
            var type = obj.GetType();
            var pairs = type.GetProperties().Where(x => x.IsMappableProperty())
                .DistinctBy(propertyInfo => propertyInfo.Name)
                .ToDictionary(
                    propertyInfo => propertyInfo.Name,
                    propertyInfo => propertyInfo.GetValue(obj, null));
            return pairs;
        }
    }
}
