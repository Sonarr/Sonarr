using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    [TableName("SceneMappings")]
    [PrimaryKey("CleanTitle", autoIncrement = false)]
    public class SceneMapping
    {
        [SubSonicPrimaryKey]
        public virtual string CleanTitle { get; set; }

        public virtual int SeriesId { get; set; }

        public virtual string SceneName { get; set; }
    }
}
