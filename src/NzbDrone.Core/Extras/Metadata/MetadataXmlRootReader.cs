using System;
using System.IO;
using System.Xml;
using NLog;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IReadMetadataXmlRoot
    {
        string GetRootElementName(string path);
    }

    public class MetadataXmlRootReader : IReadMetadataXmlRoot
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public MetadataXmlRootReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public string GetRootElementName(string path)
        {
            try
            {
                using var stream = _diskProvider.OpenReadStream(path);
                using var reader = XmlReader.Create(stream, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreWhitespace = true
                });

                return reader.MoveToContent() == XmlNodeType.Element ? reader.LocalName : null;
            }
            catch (Exception ex) when (ex is XmlException or IOException or UnauthorizedAccessException)
            {
                _logger.Debug(ex, "Unable to read XML root element: {0}", path);
                return null;
            }
        }
    }
}
