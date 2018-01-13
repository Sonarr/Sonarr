import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as seriesIndexActions from 'Store/Actions/seriesIndexActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.items,
    (state) => state.seriesIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRemoveCustomFilterPress(payload) {
      dispatch(seriesIndexActions.removeSeriesCustomFilter(payload));
    },

    onSaveCustomFilterPress(payload) {
      dispatch(seriesIndexActions.saveSeriesCustomFilter(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
