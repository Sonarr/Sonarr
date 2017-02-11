import $ from 'jquery';
import updateEpisodes from 'Utilities/Episode/updateEpisodes';

function createBatchToggleEpisodeMonitoredHandler(section, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        episodeIds,
        monitored
      } = payload;

      const state = getFromState(getState());

      updateEpisodes(dispatch, section, state.items, episodeIds, {
        isSaving: true
      });

      const promise = $.ajax({
        url: '/episode/monitor',
        method: 'PUT',
        data: JSON.stringify({ episodeIds, monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateEpisodes(dispatch, section, state.items, episodeIds, {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateEpisodes(dispatch, section, state.items, episodeIds, {
          isSaving: false
        });
      });
    };
  };
}

export default createBatchToggleEpisodeMonitoredHandler;
