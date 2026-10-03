import classNames from 'classnames';
import React, { SyntheticEvent, useCallback } from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import { useMenu } from './MenuContext';
import styles from './MenuItem.module.css';

export interface MenuItemProps extends LinkProps {
  className?: string;
  children: React.ReactNode;
  isDisabled?: boolean;
}

function MenuItem({
  className = styles.menuItem,
  children,
  isDisabled = false,
  onPress,
  ...otherProps
}: MenuItemProps) {
  const menu = useMenu();

  const handlePress = useCallback(
    (event: SyntheticEvent) => {
      menu?.closeMenu();
      onPress?.(event);
    },
    [menu, onPress]
  );

  return (
    <Link
      className={classNames(className, isDisabled && styles.isDisabled)}
      isDisabled={isDisabled}
      onPress={handlePress}
      {...otherProps}
    >
      {children}
    </Link>
  );
}

export default MenuItem;
