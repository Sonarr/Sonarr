import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllSeriesSelector(),
    (state) => state.settings.importLists.items,
    (id, series, lists) => {
      if (!id) {
        return false;
      }

      if (_.some(series, { [profileProp]: id }) || _.some(lists, { [profileProp]: id })) {
      	return true;
      }

      return false;
    }
  );
}

export default createProfileInUseSelector;
