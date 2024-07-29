import classNames from 'classnames';
import React, { ComponentProps } from 'react';
import { sizes } from 'Helpers/Props';
import { Size } from 'Helpers/Props/sizes';
import styles from './FieldSet.css';

interface FieldSetProps {
  size?: Size;
  legend?: ComponentProps<'legend'>['children'];
  children?: React.ReactNode;
}

function FieldSet({ size = sizes.MEDIUM, legend, children }: FieldSetProps) {
  return (
    <fieldset className={styles.fieldSet}>
      <legend
        className={classNames(
          styles.legend,
          size === sizes.SMALL && styles.small
        )}
      >
        {legend}
      </legend>
      {children}
    </fieldset>
  );
}

export default FieldSet;
