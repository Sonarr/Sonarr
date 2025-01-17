import React from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import FilterMenu from 'Components/Menu/FilterMenu';
import SeriesIndexFilterModal from 'Series/Index/SeriesIndexFilterModal';

interface SeriesIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect: (filter: number | string) => void;
}

function SeriesIndexFilterMenu(props: SeriesIndexFilterMenuProps) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect,
  } = props;

  return (
    <FilterMenu
      alignMenu="right"
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={SeriesIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

export default SeriesIndexFilterMenu;
