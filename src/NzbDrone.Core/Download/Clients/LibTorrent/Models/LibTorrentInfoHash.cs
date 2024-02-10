using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.LibTorrent.Models
{
    // TODO: Figure out correct json serilization of `null` values, currently setting them to ""
    public class LibTorrentInfoHash
    {
        public string Hash { get; set; }
        public string Status { get; set; }

        public LibTorrentInfoHash(string hash, string status)
        {
            Hash = hash;
            Status = status;
        }

        public LibTorrentInfoHash(IList<string> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            Hash = list[0];
            Status = list[1];
        }

        public ICollection<string> ToList()
        {
            var hash = Hash ?? string.Empty;
            var status = Status ?? string.Empty;
            return new[] { hash, status };
        }
    }

    public class LibTorrentInfoHashConverter : JsonConverter<LibTorrentInfoHash>
    {
        public override LibTorrentInfoHash ReadJson(
            JsonReader reader,
            Type objectType,
            LibTorrentInfoHash existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                return existingValue;
            }

            return new (serializer.Deserialize<List<string>>(reader));
        }

        public override void WriteJson(
            JsonWriter writer,
            LibTorrentInfoHash value,
            JsonSerializer serializer)
        {
           serializer.Serialize(writer, value?.ToList() ?? null);
        }

        public override bool CanRead => true;
    }
}
