using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Exceptions
{
    public class IndexerException : NzbDroneException
    {
        private readonly IndexerResponse _indexerResponse;

        public IndexerException(IndexerResponse response, string message, params object[] args)
            : base(message, args)
        {
        }

        public IndexerException(IndexerResponse response, string message)
            : base(message)
        {
        }

        public IndexerResponse Response
        {
            get { return _indexerResponse; }
        }
    }
}
