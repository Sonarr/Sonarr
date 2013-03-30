using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data
{
    /// <summary>
    /// Works in conjunction with the UnitOfWork to create a new 
    /// shared context that will preserve a single IDataMapper.
    /// </summary>
    public class UnitOfWorkSharedContext : IDisposable
    {
        private UnitOfWork _mgr;
        private bool _isParentContext;

        public UnitOfWorkSharedContext(UnitOfWork mgr)
        {
            _mgr = mgr;

            if (_mgr.IsShared)
            {
                _isParentContext = false;
            }
            else
            {
                _isParentContext = true;
                _mgr.IsShared = true;
            }
        }

        public void Dispose()
        {
            if (_isParentContext)
            {
                _mgr.IsShared = false;
                _mgr.Dispose();
            }
        }
    }
}
