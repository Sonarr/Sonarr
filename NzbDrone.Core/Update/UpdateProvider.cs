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
        UpdatePackage GetAvailableUpdate(Version currentVersion);
        Dictionary<DateTime, string> UpdateLogFile();
    }
}

public class UpdateService : IUpdateService
{
    private readonly IAvailableUpdateService _availableUpdateService;
    private readonly EnvironmentProvider _environmentProvider;

    private readonly DiskProvider _diskProvider;
    private readonly Logger _logger;


    public UpdateService(IAvailableUpdateService availableUpdateService, EnvironmentProvider environmentProvider, DiskProvider diskProvider, Logger logger)
    {
        _availableUpdateService = availableUpdateService;
        _environmentProvider = environmentProvider;
        _diskProvider = diskProvider;
        _logger = logger;
    }

    public UpdatePackage GetAvailableUpdate(Version currentVersion)
    {
        var latestAvailable = _availableUpdateService.GetAvailablePackages().OrderByDescending(c => c.Version).FirstOrDefault();

        if (latestAvailable != null && latestAvailable.Version > currentVersion)
        {
            _logger.Debug("An update is available ({0}) => ({1})", currentVersion, latestAvailable.Version);
            return latestAvailable;
        }

        _logger.Trace("No updates available");
        return null;
    }

    public Dictionary<DateTime, string> UpdateLogFile()
    {
        var list = new Dictionary<DateTime, string>();
        CultureInfo provider = CultureInfo.InvariantCulture;

        if (_diskProvider.FolderExists(_environmentProvider.GetUpdateLogFolder()))
        {
            var files = _diskProvider.GetFiles(_environmentProvider.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly).ToList();

            foreach (var file in files.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name))
            {
                list.Add(DateTime.ParseExact(file.Name.Replace(file.Extension, string.Empty), "yyyy.MM.dd-H-mm", provider), file.FullName);
            }
        }

        return list;
    }
}

