using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    [JsonConverter(typeof(UTorrentTorrentJsonConverter))]
    public class UTorrentTorrent
    {
        public string Hash { get; set; }
        public UTorrentTorrentStatus Status { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public double Progress { get; set; }
        public long Downloaded { get; set; }
        public long Uploaded { get; set; }
        public double Ratio { get; set; }
        public int UploadSpeed { get; set; }
        public int DownloadSpeed { get; set; }

        public int Eta { get; set; }
        public string Label { get; set; }
        public int PeersConnected { get; set; }
        public int PeersInSwarm { get; set; }
        public int SeedsConnected { get; set; }
        public int SeedsInSwarm { get; set; }
        public double Availablity { get; set; }
        public int TorrentQueueOrder { get; set; }
        public long Remaining { get; set; }
        public string DownloadUrl { get; set; }

        public object RssFeedUrl { get; set; }
        public object StatusMessage { get; set; }
        public object StreamId { get; set; }
        public object DateAdded { get; set; }
        public object DateCompleted { get; set; }
        public object AppUpdateUrl { get; set; }
        public string RootDownloadPath { get; set; }
        public object Unknown27 { get; set; }
        public object Unknown28 { get; set; }
    }

    public class UTorrentTorrentJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UTorrentTorrent);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new UTorrentTorrent();

            result.Hash = reader.ReadAsString();
            result.Status = (UTorrentTorrentStatus)reader.ReadAsInt32();
            result.Name = reader.ReadAsString();
            reader.Read();
            result.Size = (long)reader.Value;
            result.Progress = (int)reader.ReadAsInt32() / 1000.0;
            reader.Read();
            result.Downloaded = (long)reader.Value;
            reader.Read();
            result.Uploaded = (long)reader.Value;
            result.Ratio = (int)reader.ReadAsInt32() / 1000.0;
            result.UploadSpeed = (int)reader.ReadAsInt32();
            result.DownloadSpeed = (int)reader.ReadAsInt32();

            result.Eta = (int)reader.ReadAsInt32();
            result.Label = reader.ReadAsString();
            result.PeersConnected = (int)reader.ReadAsInt32();
            result.PeersInSwarm = (int)reader.ReadAsInt32();
            result.SeedsConnected = (int)reader.ReadAsInt32();
            result.SeedsInSwarm = (int)reader.ReadAsInt32();
            result.Availablity = (int)reader.ReadAsInt32() / 65536.0;
            result.TorrentQueueOrder = (int)reader.ReadAsInt32();
            reader.Read();
            result.Remaining = (long)reader.Value;

            reader.Read();

            // Builds before 25406 don't return the remaining items.

            if (reader.TokenType != JsonToken.EndArray)
            {
                result.DownloadUrl = (string)reader.Value;

                reader.Read();
                result.RssFeedUrl = reader.Value;
                reader.Read();
                result.StatusMessage = reader.Value;
                reader.Read();
                result.StreamId = reader.Value;
                reader.Read();
                result.DateAdded = reader.Value;
                reader.Read();
                result.DateCompleted = reader.Value;
                reader.Read();
                result.AppUpdateUrl = reader.Value;
                result.RootDownloadPath = reader.ReadAsString();
                reader.Read();
                result.Unknown27 = reader.Value;
                reader.Read();
                result.Unknown28 = reader.Value;

                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
