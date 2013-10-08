using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NLog;

namespace NzbDrone.Common
{
    public interface IArchiveService
    {
        void Extract(string compressedFile, string destination);
    }

    public class ArchiveService : IArchiveService
    {
        private readonly Logger _logger;

        public ArchiveService(Logger logger)
        {
            _logger = logger;
        }

        public void Extract(string compressedFile, string destination)
        {
            _logger.Trace("Extracting archive [{0}] to [{1}]", compressedFile, destination);

            using (var fileStream = File.OpenRead(compressedFile))
            {
                var zipFile = new ZipFile(fileStream);

                _logger.Debug("Validating Archive {0}", compressedFile);

                if (!zipFile.TestArchive(true, TestStrategy.FindFirstError, OnZipError))
                {
                    throw new IOException(string.Format("File {0} failed archive validation.", compressedFile));
                }

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue; // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096]; // 4K is optimum
                    Stream zipStream = zipFile.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(destination, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }

            _logger.Trace("Extraction complete.");
        }

        private void OnZipError(TestStatus status, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.Error("File {0} failed zip validation. {1}", status.File.Name, message);
            }
        }
    }
}