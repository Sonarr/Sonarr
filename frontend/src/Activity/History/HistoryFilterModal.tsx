import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setHistoryFilter } from 'Store/Actions/historyActions';

function createHistorySelector() {
  return createSelector(
    (state: AppState) => state.history.items,
    (queueItems) => {
      return queueItems;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.history.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

type HistoryFilterModalProps = FilterModalProps<History>;

export default function HistoryFilterModal(props: HistoryFilterModalProps) {
  const sectionItems = useSelector(createHistorySelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: { selectedFilterKey: string | number }) => {
      dispatch(setHistoryFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType="history"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
