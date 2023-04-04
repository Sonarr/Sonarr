import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setSeriesFilter } from 'Store/Actions/seriesIndexActions';

function createSeriesSelector() {
  return createSelector(
    (state: AppState) => state.series.items,
    (series) => {
      return series;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.seriesIndex.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

interface SeriesIndexFilterModalProps {
  isOpen: boolean;
}

export default function SeriesIndexFilterModal(
  props: SeriesIndexFilterModalProps
) {
  const sectionItems = useSelector(createSeriesSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'series';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setSeriesFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      // TODO: Don't spread all the props
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
