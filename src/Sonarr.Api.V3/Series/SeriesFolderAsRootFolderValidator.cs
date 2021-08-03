using System;
using System.IO;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace Sonarr.Api.V3.Series
{
    public class SeriesFolderAsRootFolderValidator : PropertyValidator
    {
        private readonly IBuildFileNames _fileNameBuilder;

        public SeriesFolderAsRootFolderValidator(IBuildFileNames fileNameBuilder)
            : base("Root folder path contains series folder")
        {
            _fileNameBuilder = fileNameBuilder;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var seriesResource = context.Instance as SeriesResource;

            if (seriesResource == null)
            {
                return true;
            }

            var rootFolderPath = context.PropertyValue.ToString();

            if (rootFolderPath.IsNullOrWhiteSpace())
            {
                return true;
            }

            var rootFolder = new DirectoryInfo(rootFolderPath).Name;
            var series = seriesResource.ToModel();
            var seriesFolder = _fileNameBuilder.GetSeriesFolder(series);

            if (seriesFolder == rootFolder)
            {
                return false;
            }

            var distance = seriesFolder.LevenshteinDistance(rootFolder);

            return distance >= Math.Max(1, seriesFolder.Length * 0.2);
        }
    }
}
