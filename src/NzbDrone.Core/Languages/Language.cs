using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Languages
{
    public class Language : IEmbeddedDocument, IEquatable<Language>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Language()
        {
        }

        private Language(int id, string name)
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

        public bool Equals(Language other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Language);
        }

        public static bool operator ==(Language left, Language right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Language left, Language right)
        {
            return !Equals(left, right);
        }

        public static Language Unknown => new Language(0, "Unknown");
        public static Language English => new Language(1, "English");
        public static Language French => new Language(2, "French");
        public static Language Spanish => new Language(3, "Spanish");
        public static Language German => new Language(4, "German");
        public static Language Italian => new Language(5, "Italian");
        public static Language Danish => new Language(6, "Danish");
        public static Language Dutch => new Language(7, "Dutch");
        public static Language Japanese => new Language(8, "Japanese");
        public static Language Icelandic => new Language(9, "Icelandic");
        public static Language Chinese => new Language(10, "Chinese");
        public static Language Russian => new Language(11, "Russian");
        public static Language Polish => new Language(12, "Polish");
        public static Language Vietnamese => new Language(13, "Vietnamese");
        public static Language Swedish => new Language(14, "Swedish");
        public static Language Norwegian => new Language(15, "Norwegian");
        public static Language Finnish => new Language(16, "Finnish");
        public static Language Turkish => new Language(17, "Turkish");
        public static Language Portuguese => new Language(18, "Portuguese");
        public static Language Flemish => new Language(19, "Flemish");
        public static Language Greek => new Language(20, "Greek");
        public static Language Korean => new Language(21, "Korean");
        public static Language Hungarian => new Language(22, "Hungarian");
        public static Language Hebrew => new Language(23, "Hebrew");
        public static Language Lithuanian => new Language(24, "Lithuanian");
        public static Language Czech => new Language(25, "Czech");
        public static Language Arabic => new Language(26, "Arabic");
        public static Language Hindi => new Language(27, "Hindi");
        public static Language Bulgarian => new Language(28, "Bulgarian");
        public static Language Malayalam => new Language(29, "Malayalam");
        public static Language Ukrainian => new Language(30, "Ukrainian");

        public static List<Language> All
        {
            get
            {
                return new List<Language>
                {
                    Unknown,
                    English,
                    French,
                    Spanish,
                    German,
                    Italian,
                    Danish,
                    Dutch,
                    Japanese,
                    Icelandic,
                    Chinese,
                    Russian,
                    Polish,
                    Vietnamese,
                    Swedish,
                    Norwegian,
                    Finnish,
                    Turkish,
                    Portuguese,
                    Flemish,
                    Greek,
                    Korean,
                    Hungarian,
                    Hebrew,
                    Lithuanian,
                    Czech,
                    Arabic,
                    Hindi,
                    Bulgarian,
                    Malayalam,
                    Ukrainian
                };
            }
        }

        private static readonly Dictionary<int, Language> Lookup = All.ToDictionary(v => v.Id);

        public static Language FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            if (!Lookup.TryGetValue(id, out var language))
            {
                throw new ArgumentException("ID does not match a known language", nameof(id));
            }

            return language;
        }

        public static explicit operator Language(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Language language)
        {
            return language.Id;
        }

        public static explicit operator Language(string lang)
        {
            var language = All.FirstOrDefault(v => v.Name.Equals(lang, StringComparison.InvariantCultureIgnoreCase));

            if (language == null)
            {
                throw new ArgumentException("Language does not match a known language", nameof(lang));
            }

            return language;
        }
    }
}
