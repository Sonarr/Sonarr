using System;

namespace Marr.Data
{
    public interface ILazyLoaded : ICloneable
    {
        bool IsLoaded { get; }
        void Prepare(Func<IDataMapper> dataMapperFactory, object parent);
        void LazyLoad();
    }

    /// <summary>
    /// Allows a field to be lazy loaded.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    public class LazyLoaded<TChild> : ILazyLoaded
    {
        protected TChild _value;

        public LazyLoaded()
        {
        }

        public LazyLoaded(TChild val)
        {
            _value = val;
            IsLoaded = true;
        }

        public TChild Value
        {
            get
            {
                LazyLoad();
                return _value;
            }
        }

        public bool IsLoaded { get; protected set; }

        public virtual void Prepare(Func<IDataMapper> dataMapperFactory, object parent)
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
            return MemberwiseClone();
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
        private Func<IDataMapper> _dbMapperFactory;

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
            _value = val;
            IsLoaded = true;
        }

        /// <summary>
        /// The second part of the initialization happens when the entity is being built.
        /// </summary>
        /// <param name="dataMapperFactory">Knows how to instantiate a new IDataMapper.</param>
        /// <param name="parent">The parent entity.</param>
        public override void Prepare(Func<IDataMapper> dataMapperFactory, object parent)
        {
            _dbMapperFactory = dataMapperFactory;
            _parent = (TParent)parent;
        }

        public override void LazyLoad()
        {
            if (!IsLoaded)
            {
                if (_condition != null && _condition(_parent))
                {
                    using (IDataMapper db = _dbMapperFactory())
                    {
                        _value = _query(db, _parent);
                    }
                }
                else
                {
                    _value = default(TChild);
                }

                IsLoaded = true;
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