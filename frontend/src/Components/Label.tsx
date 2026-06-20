import classNames from 'classnames';
import React, { ComponentProps, ReactNode } from 'react';
import Icon, { IconName } from 'Components/Icon';
import { kinds, sizes } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { Size } from 'Helpers/Props/sizes';
import styles from './Label.css';

export interface LabelProps extends ComponentProps<'span'> {
  kind?: Extract<Kind, keyof typeof styles>;
  size?: Extract<Size, keyof typeof styles>;
  outline?: boolean;
  icon?: IconName;
  iconFilled?: boolean;
  interactive?: boolean;
  children: ReactNode;
}

export default function Label({
  className = styles.label,
  kind = kinds.DEFAULT,
  size = sizes.SMALL,
  outline = false,
  icon,
  iconFilled = false,
  interactive = false,
  children,
  ...otherProps
}: LabelProps) {
  return (
    <span
      className={classNames(
        className,
        styles[kind],
        styles[size],
        outline && styles.outline,
        icon && styles.hasIcon,
        interactive && styles.interactive
      )}
      {...otherProps}
    >
      {icon ? <Icon name={icon} size={14} filled={iconFilled} /> : null}
      {children}
    </span>
  );
}
