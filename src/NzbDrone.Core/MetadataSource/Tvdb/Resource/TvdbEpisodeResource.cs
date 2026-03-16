namespace NzbDrone.Core.MetadataSource.Tvdb.Resource
{
    public class TvdbEpisodeResource
    {
        public int Id { get; set; }
        public int? SeriesId { get; set; }
        public string Name { get; set; }
        public int? SeasonNumber { get; set; }
        public int? Number { get; set; }
        public int? AbsoluteNumber { get; set; }
        public string Aired { get; set; }
        public string Overview { get; set; }
        public int? Runtime { get; set; }
        public string ProductionCode { get; set; }
        public bool? IsMovie { get; set; }
        public string FinaleType { get; set; }
    }
}
