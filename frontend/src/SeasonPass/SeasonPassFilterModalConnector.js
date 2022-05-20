import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setSeasonPassFilter } from 'Store/Actions/seasonPassActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.items,
    (state) => state.seasonPass.filterBuilderProps,
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
  dispatchSetFilter: setSeasonPassFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
