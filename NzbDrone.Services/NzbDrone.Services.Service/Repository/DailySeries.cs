using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;

namespace NzbDrone.Services.Service.Repository
{
    [TableName("DailySeries")]
    [PrimaryKey("Id", autoIncrement = false)]
    public class DailySeries
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}