import React, { useCallback } from 'react';
import { SetFilter } from 'Components/Filter/Filter';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import {
  getSelectedFilterKeyOption,
  setReleaseOption,
} from './releaseOptionsStore';
import useReleases, { FILTER_BUILDER, Release } from './useReleases';

interface InteractiveSearchFilterModalProps extends FilterModalProps<Release> {
  searchPayload: InteractiveSearchPayload;
}

export default function InteractiveSearchFilterModal({
  searchPayload,
  ...otherProps
}: InteractiveSearchFilterModalProps) {
  const { data } = useReleases(searchPayload);

  const handleFilterSelect = useCallback(
    (selectedFilter: SetFilter) => {
      setReleaseOption(
        getSelectedFilterKeyOption(searchPayload),
        selectedFilter.selectedFilterKey
      );
    },
    [searchPayload]
  );

  return (
    <FilterModal
      {...otherProps}
      sectionItems={data}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="releases"
      dispatchSetFilter={handleFilterSelect}
    />
  );
}
