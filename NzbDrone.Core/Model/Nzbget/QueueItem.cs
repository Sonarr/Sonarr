using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Model.Nzbget
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
                ParseResult = Parser.ParseTitle(value.Replace("DUPLICATE / ", String.Empty));
            }
        }

        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public Int32 RemainingSizeMb { get; set; }

        public EpisodeParseResult ParseResult { private set; get; }
    }
}
