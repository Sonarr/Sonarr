import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllSeriesSelector(),
    (id, series) => {
      if (!id) {
        return false;
      }

      return _.some(series, { [profileProp]: id });
    }
  );
}

export default createProfileInUseSelector;
