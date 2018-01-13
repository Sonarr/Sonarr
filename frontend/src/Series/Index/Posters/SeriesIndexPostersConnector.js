import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import SeriesIndexPosters from './SeriesIndexPosters';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesIndex.posterOptions,
    createClientSideCollectionSelector('series', 'seriesIndex'),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (posterOptions, series, uiSettings, dimensions) => {
      return {
        posterOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...series
      };
    }
  );
}

export default connect(createMapStateToProps)(SeriesIndexPosters);
