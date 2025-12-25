using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace NzbDrone.Core.Datastore
{
    public static class SqlMapperExtensions
    {
        public static IEnumerable<T> Query<T>(this IDatabase db, string sql, object param = null)
        {
            using (var conn = db.OpenConnection())
            {
                IEnumerable<T> items;
                try
                {
                    items = SqlMapper.Query<T>(conn, sql, param);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }

                if (TableMapping.Mapper.LazyLoadList.TryGetValue(typeof(T), out var lazyProperties))
                {
                    foreach (var item in items)
                    {
                        ApplyLazyLoad(db, item, lazyProperties);
                    }
                }

                return items;
            }
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            TReturn MapWithLazy(TFirst first, TSecond second)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                return map(first, second);
            }

            using (var conn = db.OpenConnection())
            {
                try
                {
                    return SqlMapper.Query<TFirst, TSecond, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                return map(first, second, third);
            }

            using (var conn = db.OpenConnection())
            {
                try
                {
                    return SqlMapper.Query<TFirst, TSecond, TThird, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third, TFourth fourth)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                ApplyLazyLoad(db, fourth);
                return map(first, second, third, fourth);
            }

            using (var conn = db.OpenConnection())
            {
                try
                {
                    return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third, TFourth fourth, TFifth fifth)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                ApplyLazyLoad(db, fourth);
                ApplyLazyLoad(db, fifth);
                return map(first, second, third, fourth, fifth);
            }

            using (var conn = db.OpenConnection())
            {
                try
                {
                    return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static IEnumerable<T> Query<T>(this IDatabase db, SqlBuilder builder)
        {
            var type = typeof(T);
            var sql = builder.Select(type).AddSelectTemplate(type);

            return db.Query<T>(sql.RawSql, sql.Parameters);
        }

        public static IEnumerable<T> QueryDistinct<T>(this IDatabase db, SqlBuilder builder)
        {
            var type = typeof(T);
            var sql = builder.SelectDistinct(type).AddSelectTemplate(type);

            return db.Query<T>(sql.RawSql, sql.Parameters);
        }

        public static IEnumerable<T> QueryJoined<T, T2>(this IDatabase db, SqlBuilder builder, Func<T, T2, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2)).AddSelectTemplate(type);

            return db.Query(sql.RawSql, mapper, sql.Parameters);
        }

        public static IEnumerable<T> QueryJoined<T, T2, T3>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3)).AddSelectTemplate(type);

            return db.Query(sql.RawSql, mapper, sql.Parameters);
        }

        public static IEnumerable<T> QueryJoined<T, T2, T3, T4>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4)).AddSelectTemplate(type);

            return db.Query(sql.RawSql, mapper, sql.Parameters);
        }

        public static IEnumerable<T> QueryJoined<T, T2, T3, T4, T5>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T5, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4), typeof(T5)).AddSelectTemplate(type);
            return db.Query(sql.RawSql, mapper, sql.Parameters);
        }

        // Async methods
        public static async Task<IEnumerable<T>> QueryAsync<T>(
            this IDatabase db,
            string sql,
            object param = null,
            CancellationToken cancellationToken = default)
        {
            using (var conn = await db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                IEnumerable<T> items;
                try
                {
                    items = await SqlMapper.QueryAsync<T>(conn, sql, param).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }

                if (TableMapping.Mapper.LazyLoadList.TryGetValue(typeof(T), out var lazyProperties))
                {
                    foreach (var item in items)
                    {
                        ApplyLazyLoad(db, item, lazyProperties);
                    }
                }

                return items;
            }
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            TReturn MapWithLazy(TFirst first, TSecond second)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                return map(first, second);
            }

            using (var conn = await db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    return await SqlMapper.QueryAsync<TFirst, TSecond, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                return map(first, second, third);
            }

            using (var conn = await db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    return await SqlMapper.QueryAsync<TFirst, TSecond, TThird, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third, TFourth fourth)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                ApplyLazyLoad(db, fourth);
                return map(first, second, third, fourth);
            }

            using (var conn = await db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    return await SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDatabase db, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            TReturn MapWithLazy(TFirst first, TSecond second, TThird third, TFourth fourth, TFifth fifth)
            {
                ApplyLazyLoad(db, first);
                ApplyLazyLoad(db, second);
                ApplyLazyLoad(db, third);
                ApplyLazyLoad(db, fourth);
                ApplyLazyLoad(db, fifth);
                return map(first, second, third, fourth, fifth);
            }

            using var conn = await db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(conn, sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
            }
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(this IDatabase db, SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type).AddSelectTemplate(type);
            return await db.QueryAsync<T>(sql.RawSql, sql.Parameters, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<T>> QueryDistinctAsync<T>(
            this IDatabase db,
            SqlBuilder builder,
            CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.SelectDistinct(type).AddSelectTemplate(type);
            return await db.QueryAsync<T>(sql.RawSql, sql.Parameters, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2>(this IDatabase db, SqlBuilder builder, Func<T, T2, T> mapper, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2)).AddSelectTemplate(type);
            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3>(
            this IDatabase db,
            SqlBuilder builder,
            Func<T, T2, T3, T> mapper,
            CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3)).AddSelectTemplate(type);
            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3, T4>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T> mapper, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4)).AddSelectTemplate(type);
            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3, T4, T5>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T5, T> mapper, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4), typeof(T5)).AddSelectTemplate(type);
            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyLazyLoad<TModel>(IDatabase db, TModel model)
        {
            if (TableMapping.Mapper.LazyLoadList.TryGetValue(typeof(TModel), out var lazyProperties))
            {
                ApplyLazyLoad(db, model, lazyProperties);
            }
        }

        private static void ApplyLazyLoad<TModel>(IDatabase db, TModel model, List<LazyLoadedProperty> lazyProperties)
        {
            if (model == null)
            {
                return;
            }

            foreach (var lazyProperty in lazyProperties)
            {
                var lazy = (ILazyLoaded)lazyProperty.LazyLoad.Clone();
                lazy.Prepare(db, model);
                lazyProperty.Property.SetValue(model, lazy);
            }
        }
    }
}
