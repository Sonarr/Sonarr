using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportResult
    {
        public ImportDecision ImportDecision { get; private set; }
        public List<String> Errors { get; private set; }

        public ImportResultType Result
        {
            get
            {
                //Approved and imported
                if (Errors.Empty()) return ImportResultType.Imported;

                //Decision was approved, but it was not imported
                if (ImportDecision.Approved) return ImportResultType.Skipped;

                //Decision was rejected
                return ImportResultType.Rejected;
            }
        }

        public ImportResult(ImportDecision importDecision, params String[] errors)
        {
            Ensure.That(importDecision, () => importDecision).IsNotNull();

            ImportDecision = importDecision;
            Errors = errors.ToList();
        }
    }
}
