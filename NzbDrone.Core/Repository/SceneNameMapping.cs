using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class SceneNameMapping
    {
        [SubSonicPrimaryKey]
        public virtual string SceneCleanName { get; set; }

        public virtual int SeriesId { get; set; }

        public virtual string SceneName { get; set; }
    }
}
