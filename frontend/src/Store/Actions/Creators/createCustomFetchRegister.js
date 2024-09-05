import { batchActions } from 'redux-batched-actions';
import { set, update } from '../baseActions';

export default function createCustomFetchRegister(section) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const subData = { value: '', errors: [], warnings: [] }

    // Simulate setting values to empty
    const data = {
      username: subData,
      password: subData,
      passwordConfirmation: subData
    };

    // Dispatch actions to update the state
    dispatch(batchActions([
      update({ section, data }),
      set({
        section,
        isFetching: false,
        isPopulated: true,
        error: null
      })
    ]));
  };
}