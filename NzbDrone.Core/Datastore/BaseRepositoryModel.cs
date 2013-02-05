using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eloquera.Client;

namespace NzbDrone.Core.Datastore
{
    public abstract class BaseRepositoryModel
    {
        [ID]
        public int Id;
    }
}
