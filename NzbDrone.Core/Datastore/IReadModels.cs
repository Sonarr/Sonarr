namespace NzbDrone.Core.Datastore
{
    public interface IReadModels<T> where T : ModelBase
    {
        T All();
        T Get(int id);
    }
}