using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    [JsonConverter(typeof(UTorrentTorrentJsonConverter))]
    public class UTorrentTorrent
    {
        public String Hash { get; set; }
        public UTorrentTorrentStatus Status { get; set; }
        public String Name { get; set; }
        public Int64 Size { get; set; }
        public Double Progress { get; set; }
        public Int64 Downloaded { get; set; }
        public Int64 Uploaded { get; set; }
        public Double Ratio { get; set; }
        public Int32 UploadSpeed { get; set; }
        public Int32 DownloadSpeed { get; set; }

        public Int32 Eta { get; set; }
        public String Label { get; set; }
        public Int32 PeersConnected { get; set; }
        public Int32 PeersInSwarm { get; set; }
        public Int32 SeedsConnected { get; set; }
        public Int32 SeedsInSwarm { get; set; }
        public Double Availablity { get; set; }
        public Int32 TorrentQueueOrder { get; set; }
        public Int64 Remaining { get; set; }
        public String DownloadUrl { get; set; }

        public Object RssFeedUrl { get; set; }
        public Object StatusMessage { get; set; }
        public Object StreamId { get; set; }
        public Object DateAdded { get; set; }
        public Object DateCompleted { get; set; }
        public Object AppUpdateUrl { get; set; }
        public String RootDownloadPath { get; set; }
        public Object Unknown27 { get; set; }
        public Object Unknown28 { get; set; }
    }

    class UTorrentTorrentJsonConverter : JsonConverter
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
            result.Size = (Int64)reader.Value;
            result.Progress = (int)reader.ReadAsInt32() / 1000.0;
            reader.Read();
            result.Downloaded = (Int64)reader.Value;
            reader.Read();
            result.Uploaded = (Int64)reader.Value;
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
            result.Remaining = (Int64)reader.Value;

            reader.Read();

            // Builds before 25406 don't return the remaining items.

            if (reader.TokenType != JsonToken.EndArray)
            {
                result.DownloadUrl = (String)reader.Value;
            
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
            
                while(reader.TokenType != JsonToken.EndArray)
                    reader.Read();
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
