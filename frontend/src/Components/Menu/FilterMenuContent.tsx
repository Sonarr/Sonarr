import React from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import FilterMenuItem from './FilterMenuItem';
import MenuContent from './MenuContent';
import MenuItem from './MenuItem';
import MenuItemSeparator from './MenuItemSeparator';

interface FilterMenuContentProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  showCustomFilters: boolean;
  onFilterSelect: (filter: number | string) => void;
  onCustomFiltersPress: () => void;
}

function FilterMenuContent({
  selectedFilterKey,
  filters,
  customFilters,
  showCustomFilters = false,
  onFilterSelect,
  onCustomFiltersPress,
  ...otherProps
}: FilterMenuContentProps) {
  return (
    <MenuContent {...otherProps}>
      {filters.map((filter) => {
        return (
          <FilterMenuItem
            key={filter.key}
            filterKey={filter.key}
            selectedFilterKey={selectedFilterKey}
            onPress={onFilterSelect}
          >
            {typeof filter.label === 'function' ? filter.label() : filter.label}
          </FilterMenuItem>
        );
      })}

      {customFilters.length > 0 ? <MenuItemSeparator /> : null}

      {customFilters.sort(sortByProp('label')).map((filter) => {
        return (
          <FilterMenuItem
            key={filter.id}
            filterKey={filter.id}
            selectedFilterKey={selectedFilterKey}
            onPress={onFilterSelect}
          >
            {filter.label}
          </FilterMenuItem>
        );
      })}

      {showCustomFilters && <MenuItemSeparator />}

      {showCustomFilters && (
        <MenuItem onPress={onCustomFiltersPress}>
          {translate('CustomFilters')}
        </MenuItem>
      )}
    </MenuContent>
  );
}

export default FilterMenuContent;
