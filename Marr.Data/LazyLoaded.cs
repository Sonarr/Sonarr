using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Marr.Data
{
    public interface ILazyLoaded : ICloneable
    {
        void Prepare(Func<IDataMapper> dbCreator, object parent);
        void LazyLoad();
    }

    /// <summary>
    /// Allows a field to be lazy loaded.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    public class LazyLoaded<TChild> : ILazyLoaded
    {
        protected TChild _child;
        protected bool _isLoaded;

        public LazyLoaded()
        {
        }

        public LazyLoaded(TChild val)
        {
            _child = val;
            _isLoaded = true;
        }

        public TChild Value
        {
            get
            {
                LazyLoad();
                return _child;
            }
        }

        public virtual void Prepare(Func<IDataMapper> dbCreator, object parent)
        { }

        public virtual void LazyLoad()
        { }

        public static implicit operator LazyLoaded<TChild>(TChild val)
        {
            return new LazyLoaded<TChild>(val);
        }
        
        public static implicit operator TChild(LazyLoaded<TChild> lazy)
        {
            return lazy.Value;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// This is the lazy loading proxy.
    /// </summary>
    /// <typeparam name="TParent">The parent entity that contains the lazy loaded entity.</typeparam>
    /// <typeparam name="TChild">The child entity that is being lazy loaded.</typeparam>
    internal class LazyLoaded<TParent, TChild> : LazyLoaded<TChild>
    {
        private TParent _parent;
        private Func<IDataMapper> _dbCreator;

        private readonly Func<IDataMapper, TParent, TChild> _query;
        private readonly Func<TParent, bool> _condition;

        internal LazyLoaded(Func<IDataMapper, TParent, TChild> query, Func<TParent, bool> condition = null)
        {
            _query = query;
            _condition = condition;
        }

        public LazyLoaded(TChild val) 
            : base(val)
        {
            _child = val;
            _isLoaded = true;
        }

        /// <summary>
        /// The second part of the initialization happens when the entity is being built.
        /// </summary>
        /// <param name="dbCreator">Knows how to instantiate a new IDataMapper.</param>
        /// <param name="parent">The parent entity.</param>
        public override void Prepare(Func<IDataMapper> dbCreator, object parent)
        {
            _dbCreator = dbCreator;
            _parent = (TParent)parent;
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
        }

        public override void LazyLoad()
        {
            if (!_isLoaded)
            {
                if (_condition != null && _condition(_parent))
                {
                    using (IDataMapper db = _dbCreator())
                    {
                        _child = _query(db, _parent);
                    }
                }

                _child = default(TChild);
                _isLoaded = true;
            }
        }

        public static implicit operator LazyLoaded<TParent, TChild>(TChild val)
        {
            return new LazyLoaded<TParent, TChild>(val);
        }

        public static implicit operator TChild(LazyLoaded<TParent, TChild> lazy)
        {
            return lazy.Value;
        }
    }

}