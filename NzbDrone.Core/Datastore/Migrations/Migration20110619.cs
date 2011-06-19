using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110619)]
    public class Migration20110619 : Migration
    {
        public override void Up()
        {
            if (Database.TableExists("Histories"))
            {
                Database.RemoveTable("Histories");
            }
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}