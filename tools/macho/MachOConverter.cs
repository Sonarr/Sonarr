using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MachOConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            if (args[0] == "info" && args.Length >= 2)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    PrintInfo(args[i]);
                }
            }
            else if (args[0] == "split" && args.Length >= 2)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    SplitFile(args[i]);
                }
            }
            else if (args[0] == "merge" && args.Length >= 4)
            {
                var sources = args.Skip(2).ToList();
                if (sources.Any(Directory.Exists))
                    MergeDir(args[1], args.Skip(2).ToList());
                else
                    MergeFile(args[1], args.Skip(2).ToList());
            }
            else
            {
                PrintUsage();
            }
        }

        static void PrintUsage()
        {
            var path = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"Usage: {path} info [path]");
            Console.WriteLine($"       {path} split [source]");
            Console.WriteLine($"       {path} merge [target] [source1] [source2]");
        }

        static void PrintInfo(string path)
        {
            var file = new MachOFile(path);
        }

        static void SplitFile(string path)
        {
            var file = new MachOFile(path);

            foreach (var entry in file.FatEntries)
            {
                var newPath = Path.ChangeExtension(path, "." + entry.cputype.ToString() + Path.GetExtension(path));

                using (var src = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var dst = new FileStream(newPath, FileMode.Create, FileAccess.Write))
                {
                    src.Seek(entry.offset, SeekOrigin.Begin);

                    var remaining = (int)entry.size;
                    var buf = new byte[64 * 1024];
                    while (remaining != 0)
                    {
                        var size = Math.Min(remaining, buf.Length);
                        src.Read(buf, 0, size);
                        dst.Write(buf, 0, size);
                        remaining -= size;
                    }
                }

                Console.WriteLine($"Wrote {entry.cputype} to {newPath}");
            }
        }

        static void MergeDir(string outPath, List<string> sources)
        {
            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var subdirs = sources.SelectMany(Directory.GetDirectories).Select(Path.GetFileName).Distinct().ToList();
            var files = sources.SelectMany(Directory.GetFiles).Select(Path.GetFileName).Distinct().ToList();

            foreach (var subdir in subdirs)
            {
                MergeDir(Path.Combine(outPath, subdir), sources.ConvertAll(v => Path.Combine(v, subdir)).Where(Directory.Exists).ToList());
            }

            foreach (var file in files)
            {
                MergeFile(Path.Combine(outPath, file), sources.ConvertAll(v => Path.Combine(v, file)).Where(File.Exists).ToList());
            }
        }

        static void MergeFile(string outPath, List<string> sources)
        {
            if (Directory.Exists(outPath))
            {
                outPath = Path.Combine(outPath, Path.GetFileName(sources[0]));
            }

            if (!MachOFile.IsValidFile(sources[0]))
            {
                File.Copy(sources[0], outPath);
                return;
            }

            var sourceItems = sources.ConvertAll(v => new MachOFile(v));

            var outFile = new MachOFile(outPath, true);

            sourceItems.ForEach(outFile.AppendFile);

            outFile.Write();
        }
    }

    class MachOFile
    {
        [Flags]
        public enum MachOCpuType : uint
        {
            VAX         = 1,
            ROMP        = 2,
            NS32032	    = 4,
            NS32332     = 5,
            MC680x0     = 6,
            I386        = 7,
            X86         = 7,
            X86_64	    = X86 | ABI64,
            MIPS        = 8,
            NS32532     = 9,
            HPPA        = 11,
            ARM         = 12,
            MC88000     = 13,
            SPARC       = 14,
            I860        = 15, // big-endian
            I860_LITTLE = 16, // little-endian
            RS6000      = 17,
            MC98000     = 18,
            POWERPC     = 18,
            ABI64       = 0x1000000,
            ABI64_32    = 0x2000000,
            MASK        = 0xff000000,
            POWERPC64   = POWERPC | ABI64,
            VEO	        = 255,
            ARM64       = ARM | ABI64,
            ARM64_32    = ARM | ABI64_32
        }

        public enum MachOCpuSubType : uint
        {

        }

        public class MachOArchEntry
        {
            public MachOCpuType cputype;
            public MachOCpuSubType cpusubtype;
            public uint filetype;
            public uint ncmds;
            public uint sizeofcmds;
            public uint flags;
            public uint reserved;
        }

        public class MachOFatEntry
        {
            public MachOCpuType cputype;
            public MachOCpuSubType cpusubtype;
            public uint offset;
            public uint size;
            public uint align;

            public string path;
            public MachOFatEntry srcentry;
            public MachOArchEntry archentry;
        }

        class BinaryReaderBigEndian : BinaryReader
        {
            public BinaryReaderBigEndian(Stream stream) : base(stream)
            {
            }

            public new int ReadInt32()
            {
                var data = base.ReadBytes(4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }

            public new short ReadInt16()
            {
                var data = base.ReadBytes(2);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt16(data, 0);
            }

            public new long ReadInt64()
            {
                var data = base.ReadBytes(8);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt64(data, 0);
            }

            public new uint ReadUInt32()
            {
                var data = base.ReadBytes(4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }

        }

        class BinaryWriterBigEndian : BinaryWriter
        {
            public BinaryWriterBigEndian(Stream stream) : base(stream, Encoding.UTF8, true)
            {
            }

            public override void Write(int value)
            {
                var data = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                base.Write(data);
            }

            public override void Write(uint value)
            {
                var data = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(data);
                base.Write(data);
            }
        }

        private string _path;
        private int _size;
        private MachOArchEntry _entry;
        private List<MachOFatEntry> _fatEntries = new List<MachOFatEntry>();

        public MachOArchEntry Entry => _entry;
        public List<MachOFatEntry> FatEntries => _fatEntries;

        public MachOFile(string path, bool create = false)
        {
            _path = path;

            if (File.Exists(_path) && !create)
            {
                _size = (int)new FileInfo(_path).Length;

                using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReaderBigEndian(stream))
                {
                    ReadFile(reader);
                }
            }
        }

        public static bool IsValidFile(string path)
        {
            if (!File.Exists(path))
                return false;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReaderBigEndian(stream))
            {
                var magic = reader.ReadUInt32();

                if (magic == 0xCAFEBABE || magic == 0xCEFAEDFE || magic == 0xCFFAEDFE)
                {
                    return true;
                }

                return false;
            }
        }

        private void ReadFile(BinaryReaderBigEndian reader)
        {
            var magic = reader.ReadUInt32();

            if (magic == 0xCAFEBABE)
            {
                ReadFatFile(reader);
                foreach (var entry in _fatEntries)
                {
                    Console.WriteLine($"Details for {entry.cputype}");
                    reader.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
                    ReadFile(reader);
                    entry.path = _path;
                    entry.archentry = _entry;
                    _entry = null;
                }
            }
            else if (magic == 0xCEFAEDFE)
            {
                ReadFileArch32(reader);
            }
            else if (magic == 0xCFFAEDFE)
            {
                ReadFileArch64(reader);
            }
            else
            {
                throw new ApplicationException($"File {_path} contains unknown Mach-O header");
            }
        }

        private void ReadFileArch32(BinaryReader reader)
        {
            _entry = new MachOArchEntry
            {
                cputype = (MachOCpuType)reader.ReadUInt32(),
                cpusubtype = (MachOCpuSubType)reader.ReadUInt32(),
                filetype = reader.ReadUInt32(),
                ncmds = reader.ReadUInt32(),
                sizeofcmds = reader.ReadUInt32(),
                flags = reader.ReadUInt32()
            };

            Console.WriteLine($"Found {_entry.cputype} filetype {_entry.filetype} flags {_entry.flags}");
        }

        private void ReadFileArch64(BinaryReader reader)
        {
            _entry = new MachOArchEntry
            {
                cputype = (MachOCpuType)reader.ReadUInt32(),
                cpusubtype = (MachOCpuSubType)reader.ReadUInt32(),
                filetype = reader.ReadUInt32(),
                ncmds = reader.ReadUInt32(),
                sizeofcmds = reader.ReadUInt32(),
                flags = reader.ReadUInt32(),
                reserved = reader.ReadUInt32()
            };

            Console.WriteLine($"Found {_entry.cputype} filetype {_entry.filetype} flags {_entry.flags}");
        }

        private void ReadFatFile(BinaryReaderBigEndian reader)
        {
            var numArchs = reader.ReadUInt32();

            Console.WriteLine($"Found Mach-O Universal with {numArchs} items");

            for (var i = 0; i < numArchs; i++)
            {
                var entry = new MachOFatEntry
                {
                    cputype = (MachOCpuType)reader.ReadUInt32(),
                    cpusubtype = (MachOCpuSubType)reader.ReadUInt32(),
                    offset = reader.ReadUInt32(),
                    size = reader.ReadUInt32(),
                    align = reader.ReadUInt32()
                };

                Console.WriteLine($"  - {entry.cputype} at offset {entry.offset} size {entry.size}");

                _fatEntries.Add(entry);
            }
        }

        static int Align(int offset, int align)
        {
            offset += (1 << align) - 1;
            offset -= offset % (1 << align);

            return offset;
        }

        public void Write()
        {
            var align = 14;
            var offset = Align(4 + FatEntries.Count * 5 * 4, align);

            // Determine offsets
            foreach (var entry in FatEntries)
            {
                entry.offset = (uint)offset;
                entry.align = (uint)align;

                offset = Align(offset + (int)entry.size, align);
            }

            if (FatEntries.Count == 0)
            {
            }
            else if (FatEntries.Count == 1)
            {
                Console.WriteLine($"Writing {_path} {FatEntries[0].cputype} from {FatEntries[0].srcentry.path}");
                File.Copy(FatEntries[0].srcentry.path, _path);
            }
            else
            {
                Console.WriteLine($"Writing {_path}:");
                using (var dst = new FileStream(_path, FileMode.Create, FileAccess.Write))
                {
                    // Write Header
                    using (var writer = new BinaryWriterBigEndian(dst))
                    {
                        writer.Write(0xCAFEBABE);
                        writer.Write(FatEntries.Count);

                        foreach (var entry in FatEntries)
                        {
                            writer.Write((uint)entry.cputype);
                            writer.Write((uint)entry.cpusubtype);
                            writer.Write(entry.offset);
                            writer.Write(entry.size);
                            writer.Write(entry.align);
                        }
                    }

                    foreach (var entry in FatEntries)
                    {
                        Console.WriteLine($"  - {entry.cputype} from {entry.srcentry.path}");
                        using (var src = new FileStream(entry.srcentry.path, FileMode.Open, FileAccess.Read))
                        {
                            dst.Seek(entry.offset, SeekOrigin.Begin);
                            src.Seek(entry.srcentry.offset, SeekOrigin.Begin);

                            var remaining = (int)entry.size;
                            var buf = new byte[64 * 1024];
                            while (remaining != 0)
                            {
                                var size = Math.Min(remaining, buf.Length);
                                src.Read(buf, 0, size);
                                dst.Write(buf, 0, size);
                                remaining -= size;
                            }
                        }
                    }
                }
            }
        }

        public void AppendEntry(MachOFatEntry entry)
        {
            if (!FatEntries.Any(v => v.cputype == entry.cputype))
            {
                FatEntries.Add(new MachOFatEntry()
                {
                    cputype = entry.cputype,
                    cpusubtype = entry.cpusubtype,
                    offset = 0,
                    size = entry.size,
                    align = entry.align,

                    srcentry = entry,
                    archentry = entry.archentry
                });
            }
        }

        public void AppendFile(MachOFile file)
        {
            if (file.Entry != null)
            {
                AppendEntry(new MachOFatEntry
                {
                    cputype = file.Entry.cputype,
                    cpusubtype = file.Entry.cpusubtype,
                    offset = 0,
                    size = (uint)file._size,
                    align = 0,
                    path = file._path,
                    archentry = file.Entry
                });
            }
            else
            {
                file.FatEntries.ForEach(AppendEntry);
            }
        }
    }
}
