import classNames from 'classnames';
import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import styles from './MenuItem.css';

export interface MenuItemProps extends LinkProps {
  className?: string;
  children: React.ReactNode;
  isDisabled?: boolean;
}

function MenuItem({
  className = styles.menuItem,
  children,
  isDisabled = false,
  tabIndex = -1,
  ...otherProps
}: MenuItemProps) {
  return (
    <Link
      className={classNames(className, isDisabled && styles.isDisabled)}
      isDisabled={isDisabled}
      role="menuitem"
      tabIndex={tabIndex}
      {...otherProps}
    >
      {children}
    </Link>
  );
}

export default MenuItem;
