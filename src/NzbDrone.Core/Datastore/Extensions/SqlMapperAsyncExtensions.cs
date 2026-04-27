using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace NzbDrone.Core.Datastore.Extensions
{
    public static class SqlMapperAsyncExtensions
    {
        public static async IAsyncEnumerable<T> QueryAsync<T>(this IDatabase db, string sql, object param = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await using var conn = await db.OpenConnectionAsync(cancellationToken);

            IAsyncEnumerable<T> items;
            try
            {
                items = conn.QueryUnbufferedAsync<T>(sql, param);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
            }

            if (TableMapping.Mapper.LazyLoadList.TryGetValue(typeof(T), out var lazyProperties))
            {
                await foreach (var item in items.WithCancellation(cancellationToken))
                {
                    ApplyLazyLoad(db, item, lazyProperties);

                    yield return item;
                }
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

            await using var conn = await db.OpenConnectionAsync(cancellationToken);

            try
            {
                return await conn.QueryAsync<TFirst, TSecond, TReturn>(sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
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

            await using var conn = await db.OpenConnectionAsync(cancellationToken);

            try
            {
                return await conn.QueryAsync<TFirst, TSecond, TThird, TReturn>(sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
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

            await using var conn = await db.OpenConnectionAsync(cancellationToken);

            try
            {
                return await conn.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
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

            await using var conn = await db.OpenConnectionAsync(cancellationToken);

            try
            {
                return await conn.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, MapWithLazy, param, transaction, buffered, splitOn, commandTimeout, commandType);
            }
            catch (Exception e)
            {
                e.Data.Add("SQL", SqlBuilderExtensions.GetSqlLogString(sql, param));
                throw;
            }
        }

        public static IAsyncEnumerable<T> QueryAsync<T>(this IDatabase db, SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.Select(type).AddSelectTemplate(type);

            return db.QueryAsync<T>(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
        }

        public static IAsyncEnumerable<T> QueryDistinctAsync<T>(this IDatabase db, SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            var sql = builder.SelectDistinct(type).AddSelectTemplate(type);

            return db.QueryAsync<T>(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2>(this IDatabase db, SqlBuilder builder, Func<T, T2, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2)).AddSelectTemplate(type);

            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3)).AddSelectTemplate(type);

            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3, T4>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4)).AddSelectTemplate(type);

            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters);
        }

        public static async Task<IEnumerable<T>> QueryJoinedAsync<T, T2, T3, T4, T5>(this IDatabase db, SqlBuilder builder, Func<T, T2, T3, T4, T5, T> mapper)
        {
            var type = typeof(T);
            var sql = builder.Select(type, typeof(T2), typeof(T3), typeof(T4), typeof(T5)).AddSelectTemplate(type);

            return await db.QueryAsync(sql.RawSql, mapper, sql.Parameters);
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
