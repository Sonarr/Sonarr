import classNames from 'classnames';
import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import styles from './MenuButton.module.css';

export interface MenuButtonProps extends Omit<LinkProps, 'onPress'> {
  className?: string;
  children: React.ReactNode;
  isDisabled?: boolean;
  onPress?: () => void;
}

function MenuButton({
  className = styles.menuButton,
  children,
  isDisabled = false,
  ...otherProps
}: MenuButtonProps) {
  return (
    <Link
      className={classNames(className, isDisabled && styles.isDisabled)}
      isDisabled={isDisabled}
      {...otherProps}
    >
      {children}
    </Link>
  );
}

export default MenuButton;
