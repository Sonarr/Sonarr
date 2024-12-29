using System;
using System.Collections.Generic;
using Workarr.Indexers;
using Workarr.Validation;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IIndexerSettings
    {
        public WorkarrValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public string BaseUrl { get; set; }

        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }
    }
}
