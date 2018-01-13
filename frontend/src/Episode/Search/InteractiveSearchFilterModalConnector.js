import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as releaseActions from 'Store/Actions/releaseActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases.items,
    (state) => state.releases.filterBuilderProps,
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
      dispatch(releaseActions.removeReleasesCustomFilter(payload));
    },

    onSaveCustomFilterPress(payload) {
      dispatch(releaseActions.saveReleasesCustomFilter(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
