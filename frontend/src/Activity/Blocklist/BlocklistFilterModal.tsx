import React, { useCallback } from 'react';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setBlocklistOption } from './blocklistOptionsStore';
import useBlocklist, { FILTER_BUILDER } from './useBlocklist';

type BlocklistFilterModalProps = FilterModalProps<History>;

export default function BlocklistFilterModal(props: BlocklistFilterModalProps) {
  const { records } = useBlocklist();

  const dispatchSetFilter = useCallback(
    ({ selectedFilterKey }: { selectedFilterKey: string | number }) => {
      setBlocklistOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  return (
    <FilterModal
      {...props}
      sectionItems={records}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="blocklist"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
