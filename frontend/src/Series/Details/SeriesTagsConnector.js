import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import SeriesTags from './SeriesTags';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createTagsSelector(),
    (series, tagList) => {
      const tags = series.tags
        .map((tagId) => tagList.find((tag) => tag.id === tagId))
        .filter((tag) => !!tag)
        .map((tag) => tag.label)
        .sort((a, b) => a.localeCompare(b));

      return {
        tags
      };
    }
  );
}

export default connect(createMapStateToProps)(SeriesTags);
