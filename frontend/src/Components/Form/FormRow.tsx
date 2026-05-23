import classNames from 'classnames';
import React, { ComponentPropsWithoutRef, ReactNode } from 'react';
import { Size } from 'Helpers/Props/sizes';
import styles from './FormRow.css';

export type FormRowLayout = 'right' | 'stacked';

interface FormRowProps extends ComponentPropsWithoutRef<'div'> {
  className?: string;
  children: ReactNode;
  size?: Extract<Size, keyof typeof styles>;
  layout?: FormRowLayout;
  advancedSettings?: boolean;
  isAdvanced?: boolean;
}

function FormRow({
  className = styles.row,
  children,
  size = 'small',
  layout = 'right',
  advancedSettings = false,
  isAdvanced = false,
  ...otherProps
}: FormRowProps) {
  if (!advancedSettings && isAdvanced) {
    return null;
  }

  return (
    <div
      className={classNames(className, styles[size], styles[layout])}
      {...otherProps}
    >
      {children}
    </div>
  );
}

export default FormRow;
