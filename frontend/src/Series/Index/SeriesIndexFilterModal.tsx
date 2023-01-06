import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setSeriesFilter } from 'Store/Actions/seriesIndexActions';

function createSeriesSelector() {
  return createSelector(
    (state) => state.series.items,
    (series) => {
      return series;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state) => state.series.items,
    (series) => {
      return series;
    }
  );
}

export default function SeriesIndexFilterModal(props) {
  const sectionItems = useSelector(createSeriesSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'series';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload) => {
      dispatch(setSeriesFilter(payload));
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
