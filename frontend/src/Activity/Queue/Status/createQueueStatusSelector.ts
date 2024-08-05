import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createQueueStatusSelector() {
  return createSelector(
    (state: AppState) => state.queue.status.isPopulated,
    (state: AppState) => state.queue.status.item,
    (state: AppState) => state.queue.options.includeUnknownSeriesItems,
    (isPopulated, status, includeUnknownSeriesItems) => {
      const {
        errors,
        warnings,
        unknownErrors,
        unknownWarnings,
        count,
        totalCount,
      } = status;

      return {
        ...status,
        isPopulated,
        count: includeUnknownSeriesItems ? totalCount : count,
        errors: includeUnknownSeriesItems ? errors || unknownErrors : errors,
        warnings: includeUnknownSeriesItems
          ? warnings || unknownWarnings
          : warnings,
      };
    }
  );
}

export default createQueueStatusSelector;
