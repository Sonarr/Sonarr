import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setSeriesEditorFilter } from 'Store/Actions/seriesEditorActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series.items,
    (state) => state.seriesEditor.filterBuilderProps,
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
  dispatchSetFilter: setSeriesEditorFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
