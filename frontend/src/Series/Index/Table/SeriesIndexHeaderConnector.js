import { connect } from 'react-redux';
import { setSeriesTableOption } from 'Store/Actions/seriesIndexActions';
import SeriesIndexHeader from './SeriesIndexHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setSeriesTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(SeriesIndexHeader);
