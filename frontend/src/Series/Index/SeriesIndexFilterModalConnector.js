import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setSeriesFilter } from 'Store/Actions/seriesIndexActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.items,
    (state) => state.seriesIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'series'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setSeriesFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
