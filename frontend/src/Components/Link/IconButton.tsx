import classNames from 'classnames';
import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import Link, { LinkProps } from './Link';
import styles from './IconButton.css';

export interface IconButtonProps
  extends Omit<LinkProps, 'name' | 'kind'>,
    Pick<IconProps, 'name' | 'kind' | 'size' | 'isSpinning' | 'filled'> {
  iconClassName?: IconProps['className'];
}

export default function IconButton({
  className = styles.button,
  iconClassName,
  name,
  kind,
  size = 12,
  isSpinning,
  filled,
  ...otherProps
}: IconButtonProps) {
  return (
    <Link
      className={classNames(
        className,
        otherProps.isDisabled && styles.isDisabled
      )}
      {...otherProps}
    >
      <Icon
        className={iconClassName}
        name={name}
        kind={kind}
        size={size}
        isSpinning={isSpinning}
        filled={filled}
        aria-hidden={true}
      />
    </Link>
  );
}
