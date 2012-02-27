using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository
{
    [TableName("PendingSceneMappings")]
    [PrimaryKey("MappingId", autoIncrement = true)]
    public class PendingSceneMapping
    {
        public int MappingId { get; set; }
        public string CleanTitle { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }

        [ResultColumn]
        public string Commands { get; set; }
    }
}