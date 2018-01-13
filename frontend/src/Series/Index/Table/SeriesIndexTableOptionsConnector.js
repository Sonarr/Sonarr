import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import SeriesIndexTableOptions from './SeriesIndexTableOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesIndex.tableOptions,
    (tableOptions) => {
      return tableOptions;
    }
  );
}

export default connect(createMapStateToProps)(SeriesIndexTableOptions);
