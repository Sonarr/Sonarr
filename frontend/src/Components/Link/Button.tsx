import classNames from 'classnames';
import React from 'react';
import { align, kinds, sizes } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { Size } from 'Helpers/Props/sizes';
import Link, { LinkProps } from './Link';
import styles from './Button.css';

export interface ButtonProps extends Omit<LinkProps, 'children' | 'size'> {
  buttonGroupPosition?: Extract<
    (typeof align.all)[number],
    keyof typeof styles
  >;
  kind?: Extract<Kind, keyof typeof styles>;
  size?: Extract<Size, keyof typeof styles>;
  children: Required<LinkProps['children']>;
}

export default function Button({
  className = styles.button,
  buttonGroupPosition,
  kind = kinds.DEFAULT,
  size = sizes.MEDIUM,
  ...otherProps
}: ButtonProps) {
  return (
    <Link
      className={classNames(
        className,
        styles[kind],
        styles[size],
        buttonGroupPosition && styles[buttonGroupPosition]
      )}
      {...otherProps}
    />
  );
}
