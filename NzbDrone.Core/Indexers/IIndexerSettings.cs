namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSetting
    {
        bool IsValid { get; }
    }


    public class NullSetting : IIndexerSetting
    {
        public bool IsValid
        {
            get
            {
                return true;
            }
        }
    }
}
