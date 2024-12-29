using Workarr.Exceptions;

namespace Workarr.DataAugmentation.Scene
{
    public class InvalidSceneMappingException : WorkarrException
    {
        public InvalidSceneMappingException(IEnumerable<SceneMapping> mappings, string releaseTitle)
            : base(FormatMessage(mappings, releaseTitle))
        {
        }

        private static string FormatMessage(IEnumerable<SceneMapping> mappings, string releaseTitle)
        {
            return string.Format("Scene Mappings contains a conflict for tvdbids {0}. Please notify Sonarr developers. ({1})", string.Join(",", mappings.Select(v => v.TvdbId.ToString())), releaseTitle);
        }
    }
}
