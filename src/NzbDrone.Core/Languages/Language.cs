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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

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

        public static Language Unknown      { get { return new Language(0, "Unknown"); } }
        public static Language English      { get { return new Language(1, "English"); } }
        public static Language French       { get { return new Language(2, "French"); } }
        public static Language Spanish      { get { return new Language(3, "Spanish"); } }
        public static Language German       { get { return new Language(4, "German"); } }
        public static Language Italian      { get { return new Language(5, "Italian"); } }
        public static Language Danish       { get { return new Language(6, "Danish"); } }
        public static Language Dutch        { get { return new Language(7, "Dutch"); } }
        public static Language Japanese     { get { return new Language(8, "Japanese"); } }
        public static Language Cantonese    { get { return new Language(9, "Cantonese"); } }
        public static Language Mandarin     { get { return new Language(10, "Mandarin"); } }
        public static Language Russian      { get { return new Language(11, "Russian"); } }
        public static Language Polish       { get { return new Language(12, "Polish"); } }
        public static Language Vietnamese   { get { return new Language(13, "Vietnamese"); } }
        public static Language Swedish      { get { return new Language(14, "Swedish"); } }
        public static Language Norwegian    { get { return new Language(15, "Norwegian"); } }
        public static Language Finnish      { get { return new Language(16, "Finnish"); } }
        public static Language Turkish      { get { return new Language(17, "Turkish"); } }
        public static Language Portuguese   { get { return new Language(18, "Portuguese"); } }
        public static Language Flemish      { get { return new Language(19, "Flemish"); } }
        public static Language Greek        { get { return new Language(20, "Greek"); } }
        public static Language Korean       { get { return new Language(21, "Korean"); } }
        public static Language Hungarian    { get { return new Language(22, "Hungarian"); } }
        public static Language Hebrew       { get { return new Language(23, "Hebrew"); } }
        public static Language Lithuanian   { get { return new Language(24, "Lithuanian"); } }
        public static Language Czech        { get { return new Language(25, "Czech"); } }

        
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
                    Cantonese,
                    Mandarin,
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
                    Czech
                };
            }
        }

        public static Language FindById(int id)
        {
            if (id == 0) return Unknown;

            Language language = All.FirstOrDefault(v => v.Id == id);

            if (language == null)
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