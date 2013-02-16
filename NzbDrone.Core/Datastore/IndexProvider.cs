using System;
using System.Collections.Generic;
using System.Linq;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public interface IProvideIndex
    {
        int Next(Type type);
    }

    public class IndexProvider : IProvideIndex
    {
        private readonly DB _db;

        private static object _lock = new object();

        public IndexProvider(DB db)
        {
            _db = db;

            if (db.IsTypeRegistered(typeof(IndexList)))
            {
                db.RegisterType(typeof(IndexList));
            }

            lock (_lock)
            {
                try
                {
                    _db.Query<IndexList>().Count();

                }
                catch (EloqueraException ex)
                {
                    _db.Store(new IndexList());
                }
            }

        }

        public int Next(Type type)
        {
            if (type == null)
            {
                throw new ArgumentException();
            }

            var key = type.Name;

            lock (_lock)
            {
                var indexList = _db.Query<IndexList>().Single();

                var indexInfo = indexList.SingleOrDefault(c => c.Type == key);

                if (indexInfo == null)
                {
                    indexInfo = new IndexInfo { Type = key };
                    indexList.Add(indexInfo);
                }

                indexInfo.Index++;

                _db.Store(indexList);

                return indexInfo.Index;
            }
        }

        public class IndexList : List<IndexInfo> { }

        public class IndexInfo
        {
            public string Type { get; set; }
            public int Index { get; set; }
        }

    }
}