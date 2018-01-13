import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingSeriesSelector from 'Store/Selectors/createExistingSeriesSelector';
import ImportSeriesSearchResult from './ImportSeriesSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingSeriesSelector(),
    (isExistingSeries) => {
      return {
        isExistingSeries
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportSeriesSearchResult);
