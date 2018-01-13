import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import SeriesIndexOverviews from './SeriesIndexOverviews';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesIndex.overviewOptions,
    createClientSideCollectionSelector('series', 'seriesIndex'),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (overviewOptions, series, uiSettings, dimensions) => {
      return {
        overviewOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...series
      };
    }
  );
}

export default connect(createMapStateToProps)(SeriesIndexOverviews);
