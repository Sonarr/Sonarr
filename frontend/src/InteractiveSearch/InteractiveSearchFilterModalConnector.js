import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setEpisodeReleasesFilter, setSeasonReleasesFilter } from 'Store/Actions/releaseActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases.items,
    (state) => state.releases.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'releases'
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchSetFilter(payload) {
      const action = props.type === 'episode' ?
        setEpisodeReleasesFilter:
        setSeasonReleasesFilter;

      dispatch(action(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
