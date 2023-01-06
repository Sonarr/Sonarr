import { createSelector } from 'reselect';

const selectTableOptions = createSelector(
  (state) => state.seriesIndex.tableOptions,
  (tableOptions) => tableOptions
);

export default selectTableOptions;
