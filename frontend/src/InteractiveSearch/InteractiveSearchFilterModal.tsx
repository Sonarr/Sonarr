import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import InteractiveSearchType from 'InteractiveSearch/InteractiveSearchType';
import {
  setEpisodeReleasesFilter,
  setSeasonReleasesFilter,
} from 'Store/Actions/releaseActions';

function createReleasesSelector() {
  return createSelector(
    (state: AppState) => state.releases.items,
    (releases) => {
      return releases;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.releases.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

interface InteractiveSearchFilterModalProps {
  isOpen: boolean;
  type: InteractiveSearchType;
}

export default function InteractiveSearchFilterModal({
  type,
  ...otherProps
}: InteractiveSearchFilterModalProps) {
  const sectionItems = useSelector(createReleasesSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'releases';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      const action =
        type === 'episode' ? setEpisodeReleasesFilter : setSeasonReleasesFilter;

      dispatch(action(payload));
    },
    [type, dispatch]
  );

  return (
    <FilterModal
      // TODO: Don't spread all the props
      {...otherProps}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
