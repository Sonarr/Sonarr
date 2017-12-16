using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public abstract class TransferProviderBase<TSettings> : ITransferProvider where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }
        public Type ConfigContract => typeof(TSettings);
        public virtual ProviderMessage Message => null;
        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();
        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();
        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public abstract bool IsAvailable(DownloadClientPath item);
        public abstract IVirtualDiskProvider GetFileSystemWrapper(DownloadClientPath item, string tempPath = null);



        protected static string ResolvePath(OsPath path, string currentParent, string newParent)
        {
            var currentParentPath = new OsPath(currentParent);
            var newParentPath = new OsPath(newParent);
            if (!currentParentPath.Contains(path))
            {
                return null;
            }

            var newPath = newParentPath + (path - currentParentPath);

            return newPath.FullPath;
        }
    }
}
