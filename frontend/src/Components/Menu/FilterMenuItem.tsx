import React, { useCallback } from 'react';
import SelectedMenuItem, { SelectedMenuItemProps } from './SelectedMenuItem';

interface FilterMenuItemProps
  extends Omit<SelectedMenuItemProps, 'isSelected' | 'onPress'> {
  filterKey: string | number;
  selectedFilterKey: string | number;
  onPress: (filter: number | string) => void;
}

function FilterMenuItem({
  filterKey,
  selectedFilterKey,
  onPress,
  ...otherProps
}: FilterMenuItemProps) {
  const handlePress = useCallback(() => {
    onPress(filterKey);
  }, [filterKey, onPress]);

  return (
    <SelectedMenuItem
      {...otherProps}
      isSelected={filterKey === selectedFilterKey}
      onPress={handlePress}
    />
  );
}

export default FilterMenuItem;
