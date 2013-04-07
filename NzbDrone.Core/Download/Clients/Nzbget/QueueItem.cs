using System;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class QueueItem
    {
        private string _title;

        public Int32 NzbId { get; set; }

        [JsonProperty(PropertyName = "NzbName")]
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                ParseResult = Parser.ParseTitle<ParseResult>(value.Replace("DUPLICATE / ", String.Empty));
            }
        }

        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public Int32 RemainingSizeMb { get; set; }

        public ParseResult ParseResult { private set; get; }
    }
}
