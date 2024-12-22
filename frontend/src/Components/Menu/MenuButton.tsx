import classNames from 'classnames';
import React from 'react';
import Link from 'Components/Link/Link';
import styles from './MenuButton.css';

export interface MenuButtonProps {
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
