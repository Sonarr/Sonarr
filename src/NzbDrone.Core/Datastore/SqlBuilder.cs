using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;

namespace NzbDrone.Core.Datastore
{
    public class SqlBuilder
    {
        private readonly Dictionary<string, Clauses> _data = new Dictionary<string, Clauses>();
        private readonly DatabaseType _databaseType;

        public SqlBuilder(DatabaseType databaseType)
        {
            _databaseType = databaseType;
        }

        public int Sequence { get; private set; }

        public DatabaseType DatabaseType => _databaseType;

        public Template AddTemplate(string sql, dynamic parameters = null) =>
            new Template(this, sql, parameters);

        public SqlBuilder Intersect(string sql, dynamic parameters = null) =>
            AddClause("intersect", sql, parameters, "\nINTERSECT\n ", "\n ", "\n", false);

        public SqlBuilder InnerJoin(string sql, dynamic parameters = null) =>
            AddClause("innerjoin", sql, parameters, "\nINNER JOIN ", "\nINNER JOIN ", "\n", false);

        public SqlBuilder LeftJoin(string sql, dynamic parameters = null) =>
            AddClause("leftjoin", sql, parameters, "\nLEFT JOIN ", "\nLEFT JOIN ", "\n", false);

        public SqlBuilder RightJoin(string sql, dynamic parameters = null) =>
            AddClause("rightjoin", sql, parameters, "\nRIGHT JOIN ", "\nRIGHT JOIN ", "\n", false);

        public SqlBuilder Where(string sql, dynamic parameters = null) =>
            AddClause("where", sql, parameters, " AND ", "WHERE ", "\n", false);

        public SqlBuilder OrWhere(string sql, dynamic parameters = null) =>
            AddClause("where", sql, parameters, " OR ", "WHERE ", "\n", true);

        public SqlBuilder OrderBy(string sql, dynamic parameters = null) =>
            AddClause("orderby", sql, parameters, " , ", "ORDER BY ", "\n", false);

        public SqlBuilder Select(string sql, dynamic parameters = null) =>
            AddClause("select", sql, parameters, " , ", "", "\n", false);

        public SqlBuilder AddParameters(dynamic parameters) =>
            AddClause("--parameters", "", parameters, "", "", "", false);

        public SqlBuilder Join(string sql, dynamic parameters = null) =>
            AddClause("join", sql, parameters, "\nJOIN ", "\nJOIN ", "\n", false);

        public SqlBuilder GroupBy(string sql, dynamic parameters = null) =>
            AddClause("groupby", sql, parameters, " , ", "\nGROUP BY ", "\n", false);

        public SqlBuilder Having(string sql, dynamic parameters = null) =>
            AddClause("having", sql, parameters, "\nAND ", "HAVING ", "\n", false);

        protected SqlBuilder AddClause(string name, string sql, object parameters, string joiner, string prefix = "", string postfix = "", bool isInclusive = false)
        {
            if (!_data.TryGetValue(name, out var clauses))
            {
                clauses = new Clauses(joiner, prefix, postfix);
                _data[name] = clauses;
            }

            clauses.Add(new Clause { Sql = sql, Parameters = parameters, IsInclusive = isInclusive });
            Sequence++;
            return this;
        }

        public class Template
        {
            private static readonly Regex _regex = new Regex(@"\/\*\*.+?\*\*\/", RegexOptions.Compiled | RegexOptions.Multiline);

            private readonly string _sql;
            private readonly SqlBuilder _builder;
            private readonly object _initParams;

            private int _dataSeq = -1; // Unresolved
            private string _rawSql;
            private object _parameters;

            public Template(SqlBuilder builder, string sql, dynamic parameters)
            {
                _initParams = parameters;
                _sql = sql;
                _builder = builder;
            }

            public string RawSql
            {
                get
                {
                    ResolveSql();
                    return _rawSql;
                }
            }

            public object Parameters
            {
                get
                {
                    ResolveSql();
                    return _parameters;
                }
            }

            private void ResolveSql()
            {
                if (_dataSeq != _builder.Sequence)
                {
                    var p = new DynamicParameters(_initParams);

                    _rawSql = _sql;

                    foreach (var pair in _builder._data)
                    {
                        _rawSql = _rawSql.Replace("/**" + pair.Key + "**/", pair.Value.ResolveClauses(p));
                    }

                    _parameters = p;

                    // replace all that is left with empty
                    _rawSql = _regex.Replace(_rawSql, "");

                    _dataSeq = _builder.Sequence;
                }
            }
        }

        private class Clause
        {
            public string Sql { get; set; }
            public object Parameters { get; set; }
            public bool IsInclusive { get; set; }
        }

        private class Clauses : List<Clause>
        {
            private readonly string _joiner;
            private readonly string _prefix;
            private readonly string _postfix;

            public Clauses(string joiner, string prefix = "", string postfix = "")
            {
                _joiner = joiner;
                _prefix = prefix;
                _postfix = postfix;
            }

            public string ResolveClauses(DynamicParameters p)
            {
                foreach (var item in this)
                {
                    p.AddDynamicParams(item.Parameters);
                }

                return this.Any(a => a.IsInclusive)
                    ? _prefix +
                      string.Join(_joiner,
                          this.Where(a => !a.IsInclusive)
                              .Select(c => c.Sql)
                              .Union(new[]
                              {
                                  " ( " +
                                  string.Join(" OR ", this.Where(a => a.IsInclusive).Select(c => c.Sql).ToArray()) +
                                  " ) "
                              }).ToArray()) + _postfix
                    : _prefix + string.Join(_joiner, this.Select(c => c.Sql).ToArray()) + _postfix;
            }
        }
    }
}
