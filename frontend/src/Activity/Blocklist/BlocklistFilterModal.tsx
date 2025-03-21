import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setBlocklistFilter } from 'Store/Actions/blocklistActions';

function createBlocklistSelector() {
  return createSelector(
    (state: AppState) => state.blocklist.items,
    (blocklistItems) => {
      return blocklistItems;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.blocklist.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

type BlocklistFilterModalProps = FilterModalProps<History>;

export default function BlocklistFilterModal(props: BlocklistFilterModalProps) {
  const sectionItems = useSelector(createBlocklistSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'blocklist';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setBlocklistFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
