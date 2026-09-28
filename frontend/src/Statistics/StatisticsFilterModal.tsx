import React, { useCallback } from 'react';
import { SetFilter } from 'Components/Filter/Filter';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setStatisticsOption } from './statisticsOptionsStore';
import { FILTER_BUILDER, Statistics } from './useStatistics';

type StatisticsFilterModalProps = FilterModalProps<Statistics>;

export default function StatisticsFilterModal(
  props: StatisticsFilterModalProps
) {
  const dispatchSetFilter = useCallback(({ selectedFilterKey }: SetFilter) => {
    setStatisticsOption('selectedFilterKey', selectedFilterKey);
  }, []);

  return (
    <FilterModal
      {...props}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="statistics"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
