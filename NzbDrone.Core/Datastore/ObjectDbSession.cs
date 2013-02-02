using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Internal;

namespace NzbDrone.Core.Datastore
{
    public interface IObjectDbSession : IObjectContainer
    {
        void Create(object obj);
        void Create(object obj, int depth);
        void SaveAll<T>(Transaction transaction, IEnumerator<T> objects);

        void Update(object obj);
        void Update(object obj, int depth);
        void UpdateAll<T>(Transaction transaction, IEnumerator<T> objects);
    }


    public class ObjectDbSession : ObjectContainerSession, IObjectDbSession
    {

        public ObjectDbSession(ObjectContainerBase server)
            : base(server, server.NewTransaction(server.SystemTransaction(), new NoCacheReferenceSystem(), false))
        {
            _transaction.SetOutSideRepresentation(this);
        }

        public override void Store(object obj)
        {
            throw new InvalidOperationException("Store is not supported. please use Create() or Update()");
        }

        public override void StoreAll(Transaction transaction, IEnumerator objects)
        {
            throw new InvalidOperationException("Store is not supported. please use Create() or Update()");

        }

        public override void Store(object obj, int depth)
        {
            throw new InvalidOperationException("Store is not supported. please use Create() or Update()");
        }

        public void Create(object obj)
        {
            ValidateCreate(obj);
            base.Store(obj);
        }

        public void Create(object obj, int depth)
        {
            ValidateCreate(obj);
            base.Store(obj, depth);
        }

        public void SaveAll<T>(Transaction transaction, IEnumerator<T> objects)
        {
            var obj = objects.ToIEnumerable().ToList();
            obj.ForEach(c => ValidateCreate(c));

            base.StoreAll(transaction, obj.GetEnumerator());
        }


        public void Update(object obj)
        {
            ValidateUpdate(obj);
            base.Store(obj);
        }

        public void Update(object obj, int depth)
        {
            ValidateUpdate(obj);
            base.Store(obj, depth);
        }

        public void UpdateAll<T>(Transaction transaction, IEnumerator<T> objects)
        {
            var obj = objects.ToIEnumerable().ToList();
            obj.ForEach(c => ValidateUpdate(c));

            base.StoreAll(transaction, obj.GetEnumerator());
        }

        public void UpdateAll(Transaction transaction, IEnumerator objects)
        {
            throw new NotImplementedException();
        }


        private void ValidateCreate(object obj)
        {
            if (IsAttached(obj))
            {
                throw new InvalidOperationException("Attempted to save an object that is already attached to database");
            }
        }

        private void ValidateUpdate(object obj)
        {
            if (!IsAttached(obj))
            {
                throw new InvalidOperationException("Attempted to update an object that is not attached to database");
            }
        }

        private bool IsAttached(object obj)
        {
            return base.Ext().GetID(obj) > 0;
        }
    }


    public static class Ext
    {
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}