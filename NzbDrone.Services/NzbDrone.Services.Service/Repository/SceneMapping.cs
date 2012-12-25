using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository
{
    [TableName("SceneMappings")]
    [PrimaryKey("CleanTitle", autoIncrement = false)]
    public class SceneMapping
    {
        public string CleanTitle { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public int Season { get; set; }
    }
}