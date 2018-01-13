import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import getSectionState from 'Utilities/State/getSectionState';
import { set, update } from '../baseActions';

function createSaveHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isSaving: true }));

    const state = getSectionState(getState(), section, true);
    const saveData = Object.assign({}, state.item, state.pendingChanges);

    const promise = $.ajax({
      url,
      method: 'PUT',
      dataType: 'json',
      data: JSON.stringify(saveData)
    });

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isSaving: false,
          saveError: null,
          pendingChanges: {}
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  };
}

export default createSaveHandler;
