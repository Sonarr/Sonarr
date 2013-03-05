using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sqo;

namespace NzbDrone.Core.Datastore
{
    public interface IObjectDatabase : IDisposable
    {
        IEnumerable<T> AsQueryable<T>();
        T Insert<T>(T obj) where T : ModelBase;
        T Update<T>(T obj) where T : ModelBase;
        IList<T> InsertMany<T>(IList<T> objects) where T : ModelBase;
        IList<T> UpdateMany<T>(IList<T> objects) where T : ModelBase;
        void Delete<T>(T obj) where T : ModelBase;
        void DeleteMany<T>(IEnumerable<T> objects) where T : ModelBase;
        void UpdateField<T>(T model, string fieldName) where T : ModelBase;
    }

    public static class SiaqodbLogger
    {
        public static void ListenTo(Siaqodb db)
        {
            db.DeletedObject += OnDeletedObject;
            db.LoadingObject += OnLoadingObject;
            db.LoadedObject += OnLoadedObject;
        }

        private static void OnLoadedObject(object sender, LoadedObjectEventArgs e)
        {
            Write("Loaded", e.Object.GetType(), e.OID);
        }

        private static void OnLoadingObject(object sender, LoadingObjectEventArgs e)
        {
            Write("Loading", e.ObjectType, e.OID);
        }

        static void OnDeletedObject(object sender, DeletedEventsArgs e)
        {
            Write("Deleted", e.ObjectType, e.OID);
        }

        private static void Write(string operation, Type modelType, int id)
        {
            var message = string.Format("{0} {1}[{2}]", operation, modelType.Name, id);
            Trace.WriteLine(message, "Siaqodb");
        }
    }

    public class SiaqodbProxy : IObjectDatabase
    {
        private readonly Siaqodb _db;

        public SiaqodbProxy(Siaqodb db)
        {
            _db = db;
            //SiaqodbConfigurator.SetRaiseLoadEvents(true);
            //SiaqodbLogger.ListenTo(_db);
        }


        public void Dispose()
        {

        }

        public IEnumerable<T> AsQueryable<T>()
        {
            return _db.LoadAllLazy<T>();
        }

        public T Insert<T>(T obj) where T : ModelBase
        {
            if (obj.Id != 0)
            {
                throw new InvalidOperationException("Attempted to insert object with existing ID as new object");
            }

            _db.StoreObject(obj);
            return obj;
        }

        public T Update<T>(T obj) where T : ModelBase
        {
            if (obj.Id == 0)
            {
                throw new InvalidOperationException("Attempted to update object without an ID");
            }

            _db.StoreObject(obj);
            return obj;
        }

        public IList<T> InsertMany<T>(IList<T> objects) where T : ModelBase
        {
            return DoMany(objects, Insert);
        }

        public IList<T> UpdateMany<T>(IList<T> objects) where T : ModelBase
        {
            return DoMany(objects, Update);

        }

        public void Delete<T>(T obj) where T : ModelBase
        {
            _db.Delete(obj);
        }

        public void DeleteMany<T>(IEnumerable<T> objects) where T : ModelBase
        {
            foreach (var o in objects)
            {
                Delete(o);
            }
        }

        public void UpdateField<T>(T model, string fieldName) where T : ModelBase
        {
            _db.StoreObjectPartially(model, fieldName);

        }

        private IList<T> DoMany<T>(IEnumerable<T> objects, Func<T, T> function) where T : ModelBase
        {
            return objects.Select(function).ToList();
        }

    }
}
