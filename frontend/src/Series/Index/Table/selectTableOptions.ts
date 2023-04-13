import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const selectTableOptions = createSelector(
  (state: AppState) => state.seriesIndex.tableOptions,
  (tableOptions) => tableOptions
);

export default selectTableOptions;
