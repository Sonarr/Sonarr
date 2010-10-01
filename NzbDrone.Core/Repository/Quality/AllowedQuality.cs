namespace NzbDrone.Core.Repository.Quality
{
    public class AllowedQuality
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public QualityTypes Quality { get; set; }
    }
}
