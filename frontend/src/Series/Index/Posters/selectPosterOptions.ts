import { createSelector } from 'reselect';

const selectPosterOptions = createSelector(
  (state) => state.seriesIndex.posterOptions,
  (posterOptions) => posterOptions
);

export default selectPosterOptions;
