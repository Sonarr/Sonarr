import { connect } from 'react-redux';
import { push } from 'react-router-redux';
import { createSelector } from 'reselect';
import jdu from 'jdu';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import SeriesSearchInput from './SeriesSearchInput';

function createCleanTagsSelector() {
  return createSelector(
    createTagsSelector(),
    (tags) => {
      return tags.map((tag) => {
        const {
          id,
          label
        } = tag;

        return {
          id,
          label,
          cleanLabel: jdu.replace(label).toLowerCase()
        };
      });
    }
  );
}

function createCleanSeriesSelector() {
  return createSelector(
    createAllSeriesSelector(),
    createCleanTagsSelector(),
    (allSeries, allTags) => {
      return allSeries.map((series) => {
        const {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles = [],
          tags = []
        } = series;

        return {
          title,
          titleSlug,
          sortTitle,
          images,
          cleanTitle: jdu.replace(title).toLowerCase(),
          alternateTitles: alternateTitles.map((alternateTitle) => {
            return {
              title: alternateTitle.title,
              cleanTitle: jdu.replace(alternateTitle.title).toLowerCase()
            };
          }),
          tags: tags.map((id) => {
            return allTags.find((tag) => tag.id === id);
          })
        };
      }).sort((a, b) => {
        if (a.cleanTitle < b.cleanTitle) {
          return -1;
        }
        if (a.cleanTitle > b.cleanTitle) {
          return 1;
        }

        return 0;
      });
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createCleanSeriesSelector(),
    (series) => {
      return {
        series
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToSeries(titleSlug) {
      dispatch(push(`${window.Sonarr.urlBase}/series/${titleSlug}`));
    },

    onGoToAddNewSeries(query) {
      dispatch(push(`${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(SeriesSearchInput);
