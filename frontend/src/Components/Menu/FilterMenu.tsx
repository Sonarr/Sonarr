import React, { useCallback, useState } from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import FilterMenuContent from './FilterMenuContent';
import Menu from './Menu';
import ToolbarMenuButton from './ToolbarMenuButton';
import styles from './FilterMenu.css';

interface FilterMenuProps {
  className?: string;
  alignMenu: 'left' | 'right';
  isDisabled?: boolean;
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  buttonComponent?: React.ElementType;
  filterModalConnectorComponent?: React.ElementType;
  filterModalConnectorComponentProps?: object;
  onFilterSelect: (filter: number | string) => void;
}

function FilterMenu({
  className = styles.filterMenu,
  isDisabled = false,
  selectedFilterKey,
  filters,
  customFilters,
  buttonComponent: ButtonComponent = ToolbarMenuButton,
  filterModalConnectorComponent: FilterModalConnectorComponent,
  filterModalConnectorComponentProps,
  onFilterSelect,
  ...otherProps
}: FilterMenuProps) {
  const [isFilterModalOpen, setIsFilterModalOpen] = useState(false);

  const showCustomFilters = !!FilterModalConnectorComponent;

  const handleCustomFiltersPress = useCallback(() => {
    setIsFilterModalOpen(true);
  }, []);

  const handleFiltersModalClose = useCallback(() => {
    setIsFilterModalOpen(false);
  }, []);

  return (
    <div>
      <Menu className={className} {...otherProps}>
        <ButtonComponent
          iconName={icons.FILTER}
          showIndicator={selectedFilterKey !== 'all'}
          text={translate('Filter')}
          isDisabled={isDisabled}
        />

        <FilterMenuContent
          selectedFilterKey={selectedFilterKey}
          filters={filters}
          customFilters={customFilters}
          showCustomFilters={showCustomFilters}
          onFilterSelect={onFilterSelect}
          onCustomFiltersPress={handleCustomFiltersPress}
        />
      </Menu>

      {showCustomFilters ? (
        <FilterModalConnectorComponent
          {...filterModalConnectorComponentProps}
          isOpen={isFilterModalOpen}
          selectedFilterKey={selectedFilterKey}
          filters={filters}
          customFilters={customFilters}
          onFilterSelect={onFilterSelect}
          onModalClose={handleFiltersModalClose}
        />
      ) : null}
    </div>
  );
}

export default FilterMenu;
