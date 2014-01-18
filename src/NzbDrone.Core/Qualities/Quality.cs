using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Converters;


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

        public static Quality Unknown     { get { return new Quality(0, "Unknown"); } }
        public static Quality SDTV        { get { return new Quality(1, "SDTV"); } }
        public static Quality DVD         { get { return new Quality(2, "DVD"); } }
        public static Quality WEBDL1080p  { get { return new Quality(3, "WEBDL-1080p"); } }
        public static Quality HDTV720p    { get { return new Quality(4, "HDTV-720p"); } }
        public static Quality WEBDL720p   { get { return new Quality(5, "WEBDL-720p"); } }
        public static Quality Bluray720p  { get { return new Quality(6, "Bluray-720p"); } }
        public static Quality Bluray1080p { get { return new Quality(7, "Bluray-1080p"); } }
        public static Quality WEBDL480p   { get { return new Quality(8, "WEBDL-480p"); } }
        public static Quality HDTV1080p   { get { return new Quality(9, "HDTV-1080p"); } }
        public static Quality RAWHD       { get { return new Quality(10, "Raw-HD"); } }
        public static Quality HDTV480p    { get { return new Quality(11, "HDTV-480p"); } }
        
        public static List<Quality> All
        {
            get
            {
                return new List<Quality>
                {
                    SDTV,
                    DVD,
                    WEBDL1080p,
                    HDTV720p,
                    WEBDL720p,
                    Bluray720p,
                    Bluray1080p,
                    WEBDL480p,
                    HDTV1080p,
                    RAWHD
                };
            }
        }

        public static HashSet<QualityDefinition> DefaultQualityDefinitions
        {
            get
            {
                return new HashSet<QualityDefinition>
                {
                    new QualityDefinition(Quality.SDTV)        { /*Title = "SDTV",        */ Weight = 1,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.WEBDL480p)   { /*Title = "WEB-DL",      */ Weight = 2,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.DVD)         { /*Title = "DVD",         */ Weight = 3,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.HDTV720p)    { /*Title = "720p HDTV",   */ Weight = 4,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.HDTV1080p)   { /*Title = "1080p HDTV",  */ Weight = 5,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.RAWHD)       { /*Title = "RawHD",       */ Weight = 6,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.WEBDL720p)   { /*Title = "720p WEB-DL", */ Weight = 7,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.Bluray720p)  { /*Title = "720p BluRay", */ Weight = 8,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.WEBDL1080p)  { /*Title = "1080p WEB-DL",*/ Weight = 9,  MinSize=0, MaxSize=100 },
                    new QualityDefinition(Quality.Bluray1080p) { /*Title = "1080p BluRay",*/ Weight = 10, MinSize=0, MaxSize=100 }
                };
            }
        }

        public static Quality FindById(int id)
        {
            if (id == 0) return Unknown;

            Quality quality = All.FirstOrDefault(v => v.Id == id);

            if (quality == null)
                throw new ArgumentException("ID does not match a known quality", "id");
                        
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