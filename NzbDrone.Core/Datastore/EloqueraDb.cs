using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public class EloqueraDb : IDisposable
    {
        private readonly IdService _idService;
        public DB Db { get; private set; }

        public EloqueraDb(DB db, IdService idService)
        {
            _idService = idService;
            Db = db;
        }

        public IEnumerable<T> AsQueryable<T>()
        {
            return Db.Query<T>();
        }

        public T Insert<T>(T obj) where T : BaseRepositoryModel
        {
            if (obj.Id != 0)
            {
                throw new InvalidOperationException("Attempted to insert object with existing ID as new object");
            }

            _idService.EnsureIds(obj, new HashSet<object>());
            Db.Store(obj);
            return obj;
        }

        public T Update<T>(T obj) where T : BaseRepositoryModel
        {
            if (obj.Id == 0)
            {
                throw new InvalidOperationException("Attempted to update object without ID");
            }

            _idService.EnsureIds(obj, new HashSet<object>());
            Db.Store(obj);
            return obj;
        }

        public IList<T> InsertMany<T>(IList<T> objects) where T : BaseRepositoryModel
        {
            _idService.EnsureIds(objects, new HashSet<object>());
            return DoMany(objects, Insert);
        }
        
        public IList<T> UpdateMany<T>(IList<T> objects) where T : BaseRepositoryModel
        {
            _idService.EnsureIds(objects, new HashSet<object>());
            return DoMany(objects, Update);
        }

        public void Delete<T>(T obj) where T : BaseRepositoryModel
        {
            if (obj.Id == 0)
            {
                throw new InvalidOperationException("Attempted to delete an object without an ID");
            }

            Db.Delete(obj);
        }

        public void DeleteMany<T>(IEnumerable<T> objects) where T : BaseRepositoryModel
        {
            foreach (var o in objects)
            {
                Delete(o);
            }
        }

        private IList<T> DoMany<T>(IEnumerable<T> objects, Func<T, T> function) where T : BaseRepositoryModel
        {
            return objects.Select(function).ToList();
        }


        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
