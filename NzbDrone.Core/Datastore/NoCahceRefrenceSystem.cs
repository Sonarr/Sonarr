using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.References;

namespace NzbDrone.Core.Datastore
{
    public class NoCahceRefrenceSystem : IReferenceSystem
    {
        private ObjectReference _hashCodeTree;
        private ObjectReference _idTree;

        internal NoCahceRefrenceSystem()
        {

        }

        public virtual void AddNewReference(ObjectReference @ref)
        {
            AddReference(@ref);
        }

        public virtual void AddExistingReference(ObjectReference @ref)
        {
            AddReference(@ref);
        }

        public virtual void Commit()
        {
            Reset();
        }



        public virtual ObjectReference ReferenceForId(int id)
        {
            if (DTrace.enabled)
                DTrace.GetYapobject.Log(id);
            if (_idTree == null)
                return null;
            if (!ObjectReference.IsValidId(id))
                return null;
            else
                return _idTree.Id_find(id);
        }

        public virtual ObjectReference ReferenceForObject(object obj)
        {
            if (_hashCodeTree == null)
                return null;
            else
                return _hashCodeTree.Hc_find(obj);
        }

        public virtual void RemoveReference(ObjectReference @ref)
        {
            if (DTrace.enabled)
                DTrace.ReferenceRemoved.Log(@ref.GetID());
            if (_hashCodeTree != null)
                _hashCodeTree = _hashCodeTree.Hc_remove(@ref);
            if (_idTree == null)
                return;
            _idTree = _idTree.Id_remove(@ref);
        }

        public virtual void Rollback()
        {
            Reset();
        }

        public virtual void TraverseReferences(IVisitor4 visitor)
        {
            if (_hashCodeTree == null)
                return;
            _hashCodeTree.Hc_traverse(visitor);
        }

        public virtual void Discarded()
        {
        }


        public void Reset()
        {
            _hashCodeTree = null;
            _idTree = null;
        }

        private void AddReference(ObjectReference @ref)
        {
            @ref.Ref_init();
            IdAdd(@ref);
            HashCodeAdd(@ref);
        }

        private void HashCodeAdd(ObjectReference @ref)
        {
            if (_hashCodeTree == null)
                _hashCodeTree = @ref;
            else
                _hashCodeTree = _hashCodeTree.Hc_add(@ref);
        }

        private void IdAdd(ObjectReference @ref)
        {
            if (DTrace.enabled)
                DTrace.IdTreeAdd.Log(@ref.GetID());
            if (_idTree == null)
                _idTree = @ref;
            else
                _idTree = _idTree.Id_add(@ref);
        }

    }
}
