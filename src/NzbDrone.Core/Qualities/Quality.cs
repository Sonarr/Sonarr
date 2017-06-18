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

        public Quality()
        {
        }

        private Quality(int id, string name)
        {
            Id = id;
            Name = name;
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

        public static Quality Unknown => new Quality(0,  "Unknown");
        public static Quality SDTV => new Quality(1,  "SDTV");
        public static Quality DVD => new Quality(2,  "DVD");
        public static Quality WEBDL1080p => new Quality(3,  "WEBDL-1080p");
        public static Quality HDTV720p => new Quality(4,  "HDTV-720p");
        public static Quality WEBDL720p => new Quality(5,  "WEBDL-720p");
        public static Quality Bluray720p => new Quality(6,  "Bluray-720p");
        public static Quality Bluray1080p => new Quality(7,  "Bluray-1080p");
        public static Quality WEBDL480p => new Quality(8,  "WEBDL-480p");
        public static Quality HDTV1080p => new Quality(9,  "HDTV-1080p");
        public static Quality RAWHD => new Quality(10, "Raw-HD");
        //public static Quality HDTV480p    { get { return new Quality(11, "HDTV-480p"); } }
        public static Quality WEBRip480p  { get { return new Quality(12, "WEBRip-480p"); } }
        //public static Quality Bluray480p  { get { return new Quality(13, "Bluray-480p"); } }
        public static Quality WEBRip720p  { get { return new Quality(14, "WEBRip-720p"); } }
        public static Quality WEBRip1080p { get { return new Quality(15, "WEBRip-1080p"); } }
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p");
        public static Quality WEBRip2160p { get { return new Quality(17, "WEBRip-2160p"); } }
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p");
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p");

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                SDTV,
                DVD,
                WEBRip480p,
                WEBDL480p,
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
                Bluray2160p
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
                new QualityDefinition(Quality.DVD)         { Weight = 4,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.HDTV720p)    { Weight = 5,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.HDTV1080p)   { Weight = 6,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.RAWHD)       { Weight = 7,  MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.WEBRip720p)  { Weight = 8,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 720p" },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 8,  MinSize = 0, MaxSize = 100,  GroupName = "WEB 720p" },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 9,  MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.WEBRip1080p) { Weight = 10, MinSize = 0, MaxSize = 100,  GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 10, MinSize = 0, MaxSize = 100,  GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 11, MinSize = 0, MaxSize = 100 },
                new QualityDefinition(Quality.HDTV2160p)   { Weight = 12, MinSize = 0, MaxSize = null },
                new QualityDefinition(Quality.WEBRip2160p) { Weight = 13, MinSize = 0, MaxSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 13, MinSize = 0, MaxSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 14, MinSize = 0, MaxSize = null }
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
    }
}