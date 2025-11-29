using System.Collections.Generic;

namespace NzbDrone.Common.TPL;

public class LockByIdPool
{
    private readonly Dictionary<int, object> _locks = new();
    private readonly object _lockObject = new();

    public object GetLock(int id)
    {
        lock (_lockObject)
        {
            if (!_locks.ContainsKey(id))
            {
                _locks[id] = new object();
            }

            return _locks[id];
        }
    }
}
