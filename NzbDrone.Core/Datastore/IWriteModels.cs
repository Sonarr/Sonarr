namespace NzbDrone.Core.Datastore
{
    public interface IWriteModels<T> where T : ModelBase
    {
        T Add(T model);
        T Update(T model);
        void Delete(int id);
    }
}