import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import CutoffUnmetRow from './CutoffUnmetRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    (series) => {
      return {
        series
      };
    }
  );
}

export default connect(createMapStateToProps)(CutoffUnmetRow);
