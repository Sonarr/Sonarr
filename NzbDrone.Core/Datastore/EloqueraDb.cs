using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDb : IDisposable
    {
        private DB _db;

        public EloqueraDb(DB db)
        {
            _db = db;
        }

        public int Create(object obj)
        {
            return Convert.ToInt32(_db.Store(obj));
        }

        public void Update(object obj)
        {
            _db.Store(obj);
        }

        public void Delete(object obj)
        {
            _db.Delete(obj);
        }

        public void DeleteAll(object obj)
        {
            _db.DeleteAll(obj);
        }

        public IEnumerable<T> Query<T>()
        {
            return _db.Query<T>();
        }

        public IEnumerable ExecuteQuery(string query)
        {
            return _db.ExecuteQuery(query);
        }

        public IEnumerable ExecuteQuery(string query, int depth)
        {
            return _db.ExecuteQuery(query, depth);
        }

        public IEnumerable ExecuteQuery(string query, int depth, Parameters parameters)
        {
            return _db.ExecuteQuery(query, depth, parameters);
        }

        public IEnumerable ExecuteQuery(string query, Parameters parameters)
        {
            return _db.ExecuteQuery(query, parameters);
        }

        public object ExecutScalar(string query)
        {
            return _db.ExecuteQuery(query);
        }

        public object ExecuteScalar(string query, int depth)
        {
            return _db.ExecuteQuery(query, depth);
        }

        public object ExecuteScalar(string query, int depth, Parameters parameters)
        {
            return _db.ExecuteQuery(query, depth, parameters);
        }

        public object ExecuteScalar(string query, Parameters parameters)
        {
            return _db.ExecuteQuery(query, parameters);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
