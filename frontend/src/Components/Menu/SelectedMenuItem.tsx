import React, { useCallback } from 'react';
import Icon, { IconName } from 'Components/Icon';
import { icons } from 'Helpers/Props';
import MenuItem, { MenuItemProps } from './MenuItem';
import styles from './SelectedMenuItem.css';

export interface SelectedMenuItemProps extends Omit<MenuItemProps, 'onPress'> {
  name?: string;
  children: React.ReactNode;
  selectedIconName?: IconName;
  isSelected: boolean;
  onPress: (name: string) => void;
}

function SelectedMenuItem({
  children,
  name,
  selectedIconName = icons.CHECK,
  isSelected,
  onPress,
  ...otherProps
}: SelectedMenuItemProps) {
  const handlePress = useCallback(() => {
    onPress(name ?? '');
  }, [name, onPress]);

  return (
    <MenuItem {...otherProps} onPress={handlePress}>
      <div className={styles.item}>
        {children}

        <Icon
          className={isSelected ? styles.isSelected : styles.isNotSelected}
          name={selectedIconName}
        />
      </div>
    </MenuItem>
  );
}

export default SelectedMenuItem;
