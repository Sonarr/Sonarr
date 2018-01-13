import { set } from '../baseActions';

function createSetServerSideCollectionFilterHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, ...payload }));
    dispatch(fetchHandler({ page: 1 }));
  };
}

export default createSetServerSideCollectionFilterHandler;
