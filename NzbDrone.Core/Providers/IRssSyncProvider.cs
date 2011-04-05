using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface IRssSyncProvider
    {
        void Begin();
    }

    public class RssSyncProvider : IRssSyncProvider
    {
        public void Begin()
        {
            
        }
    }
}
