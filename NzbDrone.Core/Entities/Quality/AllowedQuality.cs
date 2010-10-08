namespace NzbDrone.Core.Entities.Quality
{
    public class AllowedQuality
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public QualityTypes Quality { get; set; }
    }
}
