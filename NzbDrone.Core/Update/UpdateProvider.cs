using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Update
{
    public interface IUpdateService
    {
        UpdatePackage GetAvailableUpdate();
        Dictionary<DateTime, string> GetUpdateLogFiles();
    }
}

public class UpdateService : IUpdateService
{
    private readonly IUpdatePackageProvider _updatePackageProvider;
    private readonly EnvironmentProvider _environmentProvider;

    private readonly DiskProvider _diskProvider;
    private readonly Logger _logger;


    public UpdateService(IUpdatePackageProvider updatePackageProvider, EnvironmentProvider environmentProvider, DiskProvider diskProvider, Logger logger)
    {
        _updatePackageProvider = updatePackageProvider;
        _environmentProvider = environmentProvider;
        _diskProvider = diskProvider;
        _logger = logger;
    }

    public UpdatePackage GetAvailableUpdate()
    {
        var latestAvailable = _updatePackageProvider.GetLatestUpdate();

        if (latestAvailable != null && latestAvailable.Version > _environmentProvider.Version)
        {
            _logger.Debug("An update is available ({0}) => ({1})", _environmentProvider.Version, latestAvailable.Version);
            return latestAvailable;
        }

        _logger.Trace("No updates available");
        return null;
    }

    public Dictionary<DateTime, string> GetUpdateLogFiles()
    {
        var list = new Dictionary<DateTime, string>();

        if (_diskProvider.FolderExists(_environmentProvider.GetUpdateLogFolder()))
        {
            var provider = CultureInfo.InvariantCulture;
            var files = _diskProvider.GetFiles(_environmentProvider.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly).ToList();

            foreach (var file in files.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name))
            {
                list.Add(DateTime.ParseExact(file.Name.Replace(file.Extension, string.Empty), "yyyy.MM.dd-H-mm", provider), file.FullName);
            }
        }

        return list;
    }
}

