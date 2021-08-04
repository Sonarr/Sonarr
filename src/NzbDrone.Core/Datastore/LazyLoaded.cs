using System;
using System.Text.Json.Serialization;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface ILazyLoaded : ICloneable
    {
        bool IsLoaded { get; }
        void Prepare(IDatabase database, object parent);
        void LazyLoad();
    }

    /// <summary>
    /// Allows a field to be lazy loaded.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    [JsonConverter(typeof(LazyLoadedConverterFactory))]
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

        public static implicit operator LazyLoaded<TChild>(TChild val)
        {
            return new LazyLoaded<TChild>(val);
        }

        public static implicit operator TChild(LazyLoaded<TChild> lazy)
        {
            return lazy.Value;
        }

        public virtual void Prepare(IDatabase database, object parent)
        {
        }

        public virtual void LazyLoad()
        {
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
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LazyLoaded<TParent, TChild>));

        private readonly Func<IDatabase, TParent, TChild> _query;
        private readonly Func<TParent, bool> _condition;

        private IDatabase _database;
        private TParent _parent;

        public LazyLoaded(TChild val)
            : base(val)
        {
            _value = val;
            IsLoaded = true;
        }

        internal LazyLoaded(Func<IDatabase, TParent, TChild> query, Func<TParent, bool> condition = null)
        {
            _query = query;
            _condition = condition;
        }

        public static implicit operator LazyLoaded<TParent, TChild>(TChild val)
        {
            return new LazyLoaded<TParent, TChild>(val);
        }

        public static implicit operator TChild(LazyLoaded<TParent, TChild> lazy)
        {
            return lazy.Value;
        }

        public override void Prepare(IDatabase database, object parent)
        {
            _database = database;
            _parent = (TParent)parent;
        }

        public override void LazyLoad()
        {
            if (!IsLoaded)
            {
                if (_condition != null && _condition(_parent))
                {
                    if (SqlBuilderExtensions.LogSql)
                    {
                        Logger.Trace($"Lazy loading {typeof(TChild)} for {typeof(TParent)}");
                        Logger.Trace("StackTrace: '{0}'", Environment.StackTrace);
                    }

                    _value = _query(_database, _parent);
                }
                else
                {
                    _value = default;
                }

                IsLoaded = true;
            }
        }
    }
}
