using System;
using System.Collections.Generic;
using System.Linq;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDb : IDisposable
    {
        private readonly DB _db;

        public EloqueraDb(DB db)
        {
            _db = db;
        }

        public IEnumerable<T> AsQueryable<T>()
        {
            return _db.Query<T>();
        }


        public T Create<T>(T obj)
        {
            _db.Store(obj);
            return obj;
        }

        public IList<T> CreateMany<T>(IEnumerable<T> objects)
        {
            return DoMany(objects, Create);
        }

        public T Update<T>(T obj)
        {
            _db.Store(obj);
            return obj;
        }

        public IList<T> UpdateMany<T>(IEnumerable<T> objects)
        {
            return DoMany(objects, Update);
        }


        public void Delete<T>(T obj)
        {
            _db.Delete(obj);
        }

        public void DeleteMany<T>(IEnumerable<T> objects)
        {
            foreach (var o in objects)
            {
                Delete(o);
            }
        }

        private IList<T> DoMany<T>(IEnumerable<T> objects, Func<T, T> function)
        {
            return objects.Select(function).ToList();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
