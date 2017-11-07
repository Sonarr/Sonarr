using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Represents a local filesystem transfer.
    class DefaultTransfer : TransferProviderBase<NullConfig>
    {
        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new TransferProviderDefinition
                {
                    Enable = true,
                    Name = "Default",
                    ImplementationName = nameof(DefaultTransfer),
                    Implementation = nameof(DefaultTransfer),
                    Settings = NullConfig.Instance
                };
            }
        }
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
