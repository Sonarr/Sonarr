import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingSeriesSelector from 'Store/Selectors/createExistingSeriesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AddNewSeriesSearchResult from './AddNewSeriesSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingSeriesSelector(),
    createDimensionsSelector(),
    (isExistingSeries, dimensions) => {
      return {
        isExistingSeries,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewSeriesSearchResult);
