namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSetting
    {
        bool IsValid { get; }
    }


    public class NullSetting : IIndexerSetting
    {
        public static NullSetting Instance = new NullSetting();

        private NullSetting()
        {

        }

        public bool IsValid
        {
            get
            {
                return true;
            }
        }
    }
}
