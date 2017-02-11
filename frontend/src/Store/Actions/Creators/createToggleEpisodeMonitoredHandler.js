import $ from 'jquery';
import updateEpisodes from 'Utilities/Episode/updateEpisodes';

function createToggleEpisodeMonitoredHandler(section, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        episodeId,
        monitored
      } = payload;

      const state = getFromState(getState());

      updateEpisodes(dispatch, section, state.items, [episodeId], {
        isSaving: true
      });

      const promise = $.ajax({
        url: `/episode/${episodeId}`,
        method: 'PUT',
        data: JSON.stringify({ monitored }),
        dataType: 'json'
      });

      promise.done(() => {
        updateEpisodes(dispatch, section, state.items, [episodeId], {
          isSaving: false,
          monitored
        });
      });

      promise.fail(() => {
        updateEpisodes(dispatch, section, state.items, [episodeId], {
          isSaving: false
        });
      });
    };
  };
}

export default createToggleEpisodeMonitoredHandler;
