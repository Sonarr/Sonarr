import classNames from 'classnames';
import React, { ComponentProps } from 'react';
import { sizes } from 'Helpers/Props';
import { Size } from 'Helpers/Props/sizes';
import styles from './FieldSet.css';

interface FieldSetProps {
  size?: Size;
  legend?: ComponentProps<'h2'>['children'];
  caption?: React.ReactNode;
  children?: React.ReactNode;
}

// Renders as <section> + <h2> rather than <fieldset> + <legend> to avoid the
// browser-default legend-overlap-border behavior.
function FieldSet({
  size = sizes.MEDIUM,
  legend,
  caption,
  children,
}: FieldSetProps) {
  return (
    <section className={styles.fieldSet}>
      <h2
        className={classNames(
          styles.legend,
          size === sizes.SMALL && styles.small
        )}
      >
        {legend}
      </h2>
      {caption ? <p className={styles.caption}>{caption}</p> : null}
      {children}
    </section>
  );
}

export default FieldSet;
