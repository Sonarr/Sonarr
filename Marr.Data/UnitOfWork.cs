using System;

namespace Marr.Data
{
    /// <summary>
    /// The UnitOfWork class can be used to manage the lifetime of an IDataMapper, from creation to disposal.
    /// When used in a "using" statement, the UnitOfWork will create and dispose an IDataMapper.
    /// When the SharedContext property is used in a "using" statement, 
    /// it will create a parent unit of work that will share a single IDataMapper with other units of work,
    /// and the IDataMapper will not be disposed until the shared context is disposed.
    /// If more than one shared context is created, the IDataMapper will be disposed when the outer most
    /// shared context is disposed.
    /// </summary>
    /// <remarks>
    /// It should be noted that the Dispose method on the UnitOfWork class only affects the managed IDataMapper.
    /// The UnitOfWork instance itself is not affected by the Dispose method.
    /// </remarks>
    public class UnitOfWork : IDisposable
    {
        private Func<IDataMapper> _dbConstructor;
        private IDataMapper _lazyLoadedDB;
        private short _transactionCount;

        public UnitOfWork(Func<IDataMapper> dbConstructor)
        {
            _dbConstructor = dbConstructor;
        }

        /// <summary>
        /// Gets an IDataMapper object whose lifetime is managed by the UnitOfWork class.
        /// </summary>
        public IDataMapper DB
        {
            get
            {
                if (_lazyLoadedDB == null)
                {
                    _lazyLoadedDB = _dbConstructor.Invoke();
                }

                return _lazyLoadedDB;
            }
        }

        /// <summary>
        /// Instructs the UnitOfWork to share a single IDataMapper instance.
        /// </summary>
        public UnitOfWorkSharedContext SharedContext
        {
            get
            {
                return new UnitOfWorkSharedContext(this);
            }
        }

        public void BeginTransaction()
        {
            // Only allow one transaction to begin
            if (_transactionCount < 1)
            {
                DB.BeginTransaction();
            }

            _transactionCount++;
        }

        public void Commit()
        {
            // Only allow the outermost transaction to commit (all nested transactions must succeed)
            if (_transactionCount == 1)
            {
                DB.Commit();
            }

            _transactionCount--;
        }

        public void RollBack()
        {
            // Any level transaction should be allowed to rollback
            DB.RollBack();

            // Throw an exception if a nested ShareContext transaction rolls back
            if (_transactionCount > 1)
            {
                throw new NestedSharedContextRollBackException();
            }

            _transactionCount--;
        }

        public void Dispose()
        {
            if (!IsShared)
            {
                ForceDispose();
            }
        }

        internal bool IsShared { get; set; }

        private void ForceDispose()
        {
            _transactionCount = 0;

            if (_lazyLoadedDB != null)
            {
                _lazyLoadedDB.Dispose();
                _lazyLoadedDB = null;
            }
        }
    }

    [Serializable]
    public class NestedSharedContextRollBackException : Exception
    {
        public NestedSharedContextRollBackException() { }
        public NestedSharedContextRollBackException(string message) : base(message) { }
        public NestedSharedContextRollBackException(string message, Exception inner) : base(message, inner) { }
        protected NestedSharedContextRollBackException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
