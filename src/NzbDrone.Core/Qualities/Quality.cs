using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QualitySource Source { get; set; }
        public int Resolution { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, QualitySource source, int resolution)
        {
            Id = id;
            Name = name;
            Source = source;
            Resolution = resolution;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        public static Quality Unknown => new Quality(0,  "Unknown", QualitySource.Unknown, 0);
        public static Quality SDTV => new Quality(1,  "SDTV", QualitySource.Television, 480);
        public static Quality DVD => new Quality(2,  "DVD", QualitySource.DVD, 480);
        public static Quality WEBDL1080p => new Quality(3,  "WEBDL-1080p", QualitySource.Web, 1080);
        public static Quality HDTV720p => new Quality(4,  "HDTV-720p", QualitySource.Television, 720);
        public static Quality WEBDL720p => new Quality(5,  "WEBDL-720p", QualitySource.Web, 720);
        public static Quality Bluray720p => new Quality(6,  "Bluray-720p", QualitySource.Bluray, 720);
        public static Quality Bluray1080p => new Quality(7,  "Bluray-1080p", QualitySource.Bluray, 1080);
        public static Quality WEBDL480p => new Quality(8,  "WEBDL-480p", QualitySource.Web, 480);
        public static Quality HDTV1080p => new Quality(9,  "HDTV-1080p", QualitySource.Television, 1080);
        public static Quality RAWHD => new Quality(10, "Raw-HD", QualitySource.TelevisionRaw, 1080);
        //public static Quality HDTV480p    { get { return new Quality(11, "HDTV-480p", QualitySource.Television, 480); } }
        public static Quality WEBRip480p  { get { return new Quality(12, "WEBRip-480p", QualitySource.WebRip, 480); } }
        public static Quality Bluray480p  { get { return new Quality(13, "Bluray-480p", QualitySource.Bluray, 480); } }
        public static Quality WEBRip720p  { get { return new Quality(14, "WEBRip-720p", QualitySource.WebRip, 720); } }
        public static Quality WEBRip1080p { get { return new Quality(15, "WEBRip-1080p", QualitySource.WebRip, 1080); } }
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p", QualitySource.Television, 2160);
        public static Quality WEBRip2160p { get { return new Quality(17, "WEBRip-2160p", QualitySource.WebRip, 2160); } }
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p", QualitySource.Web, 2160);
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p", QualitySource.Bluray, 2160);
        public static Quality Bluray1080pRemux => new Quality(20,  "Bluray-1080p Remux", QualitySource.BlurayRaw, 1080);
        public static Quality Bluray2160pRemux => new Quality(21, "Bluray-2160p Remux", QualitySource.BlurayRaw, 2160);

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                SDTV,
                DVD,
                WEBRip480p,
                WEBDL480p,
                Bluray480p,
                HDTV720p,
                WEBRip720p,
                WEBDL720p,
                Bluray720p,
                Bluray1080p,
                HDTV1080p,
                WEBRip1080p,
                WEBDL1080p,
                RAWHD,
                HDTV2160p,
                WEBRip2160p,
                WEBDL2160p,
                Bluray2160p,
                Bluray1080pRemux,
                Bluray2160pRemux
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.SDTV)        { Weight = 2,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.WEBRip480p)  { Weight = 3,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 480p" },
                new QualityDefinition(Quality.WEBDL480p)   { Weight = 3,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 480p" },
                new QualityDefinition(Quality.DVD)         { Weight = 4,  MinSize = 0, MaxSize = 100,  GroupName = "DVD" },
                new QualityDefinition(Quality.Bluray480p)  { Weight = 5,  MinSize = 0, MaxSize = 100,  GroupName = "DVD" },
                new QualityDefinition(Quality.HDTV720p)    { Weight = 6,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.HDTV1080p)   { Weight = 7,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.RAWHD)       { Weight = 8,  MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.WEBRip720p)  { Weight = 9,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 720p" },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 9,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 720p" },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 10, MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.WEBRip1080p) { Weight = 11, MinSize = 0, MaxSize = 100,  GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 11, MinSize = 0, MaxSize = 100,  GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 12, MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.Bluray1080pRemux) { Weight = 13, MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.HDTV2160p)   { Weight = 14, MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.WEBRip2160p) { Weight = 15, MinSize = 0, MaxSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 16, MinSize = 0, MaxSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 16, MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.Bluray2160pRemux) { Weight = 17, MinSize = 0, MaxSize = null }
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;

        public static Quality FindById(int id)
        {
            if (id == 0) return Unknown;

            var quality = AllLookup[id];

            if (quality == null)
                throw new ArgumentException("ID does not match a known quality", nameof(id));
                        
            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }

        public static Quality FindBySourceAndResolution(QualitySource source, int resolution)
        {
            return All.SingleOrDefault(q => q.Source == source && q.Resolution == resolution);
        }
    }
}
