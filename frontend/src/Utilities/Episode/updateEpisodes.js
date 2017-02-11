import _ from 'lodash';
import { update } from 'Store/Actions/baseActions';

function updateEpisodes(dispatch, section, episodes, episodeIds, options) {
  const data = _.reduce(episodes, (result, item) => {
    if (episodeIds.indexOf(item.id) > -1) {
      result.push({
        ...item,
        ...options
      });
    } else {
      result.push(item);
    }

    return result;
  }, []);

  dispatch(update({ section, data }));
}

export default updateEpisodes;
