import { createSelector } from 'reselect';
import createDeepEqualSelector from './createDeepEqualSelector';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('series', uiSection),
    (series) => {
      const items = series.items.map((s) => {
        const {
          id,
          sortTitle
        } = s;

        return {
          id,
          sortTitle
        };
      });

      return {
        ...series,
        items
      };
    }
  );
}

function createSeriesClientSideCollectionItemsSelector(uiSection) {
  return createDeepEqualSelector(
    createUnoptimizedSelector(uiSection),
    (series) => series
  );
}

export default createSeriesClientSideCollectionItemsSelector;
