namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class SQLiteColumn
    {
        public string Name { get; set; }
        public string Schema { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Name, Schema);
        }
    }
}