using System.IO;
using System.Linq;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Parser;

namespace NzbDrone.Api.FrontendModule
{
    public interface ICompileLess
    {
        string Compile(string filePath);
    }

    public class LessCompiler : ICompileLess
    {

        public string Compile(string filePath)
        {
            var parser = new Parser()
                {
                        Importer = new Importer(new LessFileReader(filePath))
                };

            var lessEngine = new LessEngine(parser, null, false, true);
            var lessContent = File.ReadAllText(filePath);
            return lessEngine.TransformToCss(lessContent, filePath);
        }


        class LessFileReader : IFileReader
        {
            private readonly string _rootFolders;

            public LessFileReader(string masterFile)
            {
                _rootFolders = new FileInfo(masterFile).Directory.FullName;
            }

            public byte[] GetBinaryFileContents(string fileName)
            {
                return File.ReadAllBytes(Path.Combine(_rootFolders, fileName));
            }

            public string GetFileContents(string fileName)
            {
                return File.ReadAllText(Path.Combine(_rootFolders, fileName));
            }

            public bool DoesFileExist(string fileName)
            {
                return File.Exists(Path.Combine(_rootFolders, fileName));
            }
        }
    }
}