import $ from 'jquery';
import updateEpisodes from 'Utilities/Episode/updateEpisodes';
import getSectionState from 'Utilities/State/getSectionState';

function createBatchToggleEpisodeMonitoredHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const {
      episodeIds,
      monitored
    } = payload;

    const state = getSectionState(getState(), section, true);

    dispatch(updateEpisodes(section, state.items, episodeIds, {
      isSaving: true
    }));

    const promise = $.ajax({
      url: '/episode/monitor',
      method: 'PUT',
      data: JSON.stringify({ episodeIds, monitored }),
      dataType: 'json'
    });

    promise.done(() => {
      dispatch(updateEpisodes(section, state.items, episodeIds, {
        isSaving: false,
        monitored
      }));

      dispatch(fetchHandler());
    });

    promise.fail(() => {
      dispatch(updateEpisodes(section, state.items, episodeIds, {
        isSaving: false
      }));
    });
  };
}

export default createBatchToggleEpisodeMonitoredHandler;
