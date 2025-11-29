import React, { useCallback } from 'react';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import Series from 'Series/Series';
import { setSeriesOption } from 'Series/seriesOptionsStore';
import useSeries, { FILTER_BUILDER } from 'Series/useSeries';

type SeriesIndexFilterModalProps = FilterModalProps<Series>;

export default function SeriesIndexFilterModal(
  props: SeriesIndexFilterModalProps
) {
  const { data: sectionItems } = useSeries();

  const dispatchSetFilter = useCallback(
    ({ selectedFilterKey }: { selectedFilterKey: string | number }) => {
      setSeriesOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="series"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
