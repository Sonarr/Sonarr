import { createSelector } from 'reselect';

const selectOverviewOptions = createSelector(
  (state) => state.seriesIndex.overviewOptions,
  (overviewOptions) => overviewOptions
);

export default selectOverviewOptions;
