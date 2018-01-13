import { createSelector } from 'reselect';

function createDimensionsSelector() {
  return createSelector(
    (state) => state.app.dimensions,
    (dimensions) => {
      return dimensions;
    }
  );
}

export default createDimensionsSelector;
