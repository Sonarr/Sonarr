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
                    throw new InvalidNzbException("No Root element", filename);
                }

                if (!nzb.Name.LocalName.Equals("nzb"))
                {
                    throw new InvalidNzbException("Invalid root element", filename);
                }

                var ns = nzb.Name.Namespace;
                var files = nzb.Elements(ns + "file").ToList();

                if (files.Empty())
                {
                    throw new InvalidNzbException("No files", filename);
                }

                foreach (var file in files)
                {
                    var groups = file.Element(ns + "groups");
                    var segments = file.Element(ns + "segments");

                    if (groups == null)
                    {
                        throw new InvalidNzbException("No groups", filename);
                    }

                    if (segments == null)
                    {
                        throw new InvalidNzbException("No segments", filename);
                    }

                    var groupCount = groups.Elements(ns + "group").Count();
                    var segmentCount = segments.Elements(ns + "segment").Count();

                    if (groupCount == 0)
                    {
                        throw new InvalidNzbException("Groups are empty", filename);
                    }

                    if (segmentCount == 0)
                    {
                        throw new InvalidNzbException("Segments are empty", filename);
                    }
                }
            }
        }
    }
}
