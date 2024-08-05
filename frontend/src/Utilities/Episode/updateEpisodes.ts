import Episode from 'Episode/Episode';
import { update } from 'Store/Actions/baseActions';

function updateEpisodes(
  section: string,
  episodes: Episode[],
  episodeIds: number[],
  options: Partial<Episode>
) {
  const data = episodes.reduce<Episode[]>((result, item) => {
    if (episodeIds.indexOf(item.id) > -1) {
      result.push({
        ...item,
        ...options,
      });
    } else {
      result.push(item);
    }

    return result;
  }, []);

  return update({ section, data });
}

export default updateEpisodes;
