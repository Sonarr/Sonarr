using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Marks the files are permanently unavailable. Perhaps useful in fire-and-forget.
    class Dummy : TransferProviderBase<NullConfig>
    {
        public override string Link
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override ValidationResult Test()
        {
            throw new NotImplementedException();
        }
    }
}
