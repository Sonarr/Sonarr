using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Migrator.Framework;

namespace Migrator.Compile
{
    public class ScriptEngine
    {
        public readonly string[] extraReferencedAssemblies;

        private readonly CodeDomProvider _provider;
        private string _codeType = "csharp";

        public ScriptEngine() : this(null, null)
        {
        }

        public ScriptEngine(string[] extraReferencedAssemblies)
            : this(null, extraReferencedAssemblies)
        {
        }

        public ScriptEngine(string codeType, string[] extraReferencedAssemblies)
        {
            if (!String.IsNullOrEmpty(codeType))
                _codeType = codeType;
            this.extraReferencedAssemblies = extraReferencedAssemblies;

            // There is currently no way to generically create a CodeDomProvider and have it work with .NET 3.5
            _provider = CodeDomProvider.CreateProvider(_codeType);
        }

        public Assembly Compile(string directory)
        {
            string[] files = GetFilesRecursive(directory);
            Console.Out.WriteLine("Compiling:");
            Array.ForEach(files, delegate(String file) { Console.Out.WriteLine(file); });

            return Compile(files);
        }

        private string[] GetFilesRecursive(string directory)
        {
            FileInfo[] files = GetFilesRecursive(new DirectoryInfo(directory));
            string[] fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i ++)
            {
                fileNames[i] = files[i].FullName;
            }
            return fileNames;
        }

        private FileInfo[] GetFilesRecursive(DirectoryInfo d)
        {
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(d.GetFiles(String.Format("*.{0}", _provider.FileExtension)));
            DirectoryInfo[] subDirs = d.GetDirectories();
            if (subDirs.Length > 0)
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    files.AddRange(GetFilesRecursive(subDir));
                }
            }

            return files.ToArray();
        }

        public Assembly Compile(params string[] files)
        {
            CompilerParameters parms = SetupCompilerParams();

            CompilerResults compileResult = _provider.CompileAssemblyFromFile(parms, files);
            if (compileResult.Errors.Count != 0)
            {
                foreach (CompilerError err in compileResult.Errors)
                {
                    Console.Error.WriteLine("{0} ({1}:{2})  {3}", err.FileName, err.Line, err.Column, err.ErrorText);
                }
            }
            return compileResult.CompiledAssembly;
        }

        private CompilerParameters SetupCompilerParams()
        {
            string migrationFrameworkPath = FrameworkAssemblyPath();
            CompilerParameters parms = new CompilerParameters();
            parms.CompilerOptions = "/t:library";
            parms.GenerateInMemory = true;
            parms.IncludeDebugInformation = true;
            parms.OutputAssembly = Path.Combine(Path.GetDirectoryName(migrationFrameworkPath), "MyMigrations.dll");

            Console.Out.WriteLine("Output assembly: " + parms.OutputAssembly);

            // Add Default referenced assemblies
            parms.ReferencedAssemblies.Add("mscorlib.dll");
            parms.ReferencedAssemblies.Add("System.dll");
            parms.ReferencedAssemblies.Add("System.Data.dll");
            parms.ReferencedAssemblies.Add(FrameworkAssemblyPath());
            if (null != extraReferencedAssemblies && extraReferencedAssemblies.Length > 0)
            {
                Array.ForEach(extraReferencedAssemblies,
                              delegate(String assemb) { parms.ReferencedAssemblies.Add(assemb); });
            }
            return parms;
        }
        
        private static string FrameworkAssemblyPath()
        {
            string path = typeof (MigrationAttribute).Module.FullyQualifiedName;
            Console.Out.WriteLine("Framework DLL: " + path);
            return path;
        }
    }
}