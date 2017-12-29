using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download
{
    public interface IValidateNzbs
    {
        void Validate(string filename, byte[] fileContent);
    }

    public class NzbValidationService : IValidateNzbs
    {
        public void Validate(string filename, byte[] fileContent)
        {
            var reader = new StreamReader(new MemoryStream(fileContent));

            using (var xmlTextReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
            {
                var xDoc = XDocument.Load(xmlTextReader);
                var nzb = xDoc.Root;

                if (nzb == null)
                {
                    throw new InvalidNzbException("Invalid NZB: No Root element [{0}]", filename);
                }

                if (!nzb.Name.LocalName.Equals("nzb"))
                {
                    throw new InvalidNzbException("Invalid NZB: Unexpected root element. Expected 'nzb' found '{0}' [{1}]", nzb.Name.LocalName, filename);
                }

                var ns = nzb.Name.Namespace;
                var files = nzb.Elements(ns + "file").ToList();

                if (files.Empty())
                {
                    throw new InvalidNzbException("Invalid NZB: No files [{0}]", filename);
                }
            }
        }
    }
}
