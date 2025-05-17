import React, { useCallback } from 'react';
import { SetFilter } from 'Components/Filter/Filter';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import Episode from 'Episode/Episode';
import { setMissingOption } from './missingOptionsStore';
import useMissing, { FILTER_BUILDER } from './useMissing';

type MissingFilterModalProps = FilterModalProps<Episode>;

export default function MissingFilterModal(props: MissingFilterModalProps) {
  const { records } = useMissing();

  const dispatchSetFilter = useCallback(({ selectedFilterKey }: SetFilter) => {
    setMissingOption('selectedFilterKey', selectedFilterKey);
  }, []);

  return (
    <FilterModal
      {...props}
      sectionItems={records}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="wanted.missing"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
