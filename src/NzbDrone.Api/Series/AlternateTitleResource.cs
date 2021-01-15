using NzbDrone.Core.DataAugmentation.Scene;

namespace NzbDrone.Api.Series
{
    public class AlternateTitleResource
    {
        public string Title { get; set; }
        public int? SeasonNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
        public string SceneOrigin { get; set; }
        public string Comment { get; set; }
    }

    public static class AlternateTitleResourceMapper
    {
        public static AlternateTitleResource ToResource(this SceneMapping sceneMapping)
        {
            if (sceneMapping == null)
            {
                return null;
            }

            return new AlternateTitleResource
            {
                Title = sceneMapping.Title,
                SeasonNumber = sceneMapping.SeasonNumber,
                SceneSeasonNumber = sceneMapping.SceneSeasonNumber,
                SceneOrigin = sceneMapping.SceneOrigin,
                Comment = sceneMapping.Comment
            };
        }
    }
}
