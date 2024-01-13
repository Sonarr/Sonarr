import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createDimensionsSelector() {
  return createSelector(
    (state: AppState) => state.app.dimensions,
    (dimensions) => {
      return dimensions;
    }
  );
}

export default createDimensionsSelector;
