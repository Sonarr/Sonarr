import classNames from 'classnames';
import React, { ComponentProps, ReactNode } from 'react';
import { kinds, sizes } from 'Helpers/Props';
import styles from './Label.css';

export interface LabelProps extends ComponentProps<'span'> {
  kind?: Extract<(typeof kinds.all)[number], keyof typeof styles>;
  size?: Extract<(typeof sizes.all)[number], keyof typeof styles>;
  outline?: boolean;
  children: ReactNode;
}

export default function Label({
  className = styles.label,
  kind = kinds.DEFAULT,
  size = sizes.SMALL,
  outline = false,
  ...otherProps
}: LabelProps) {
  return (
    <span
      className={classNames(
        className,
        styles[kind],
        styles[size],
        outline && styles.outline
      )}
      {...otherProps}
    />
  );
}
