import React, { ComponentPropsWithoutRef } from 'react';
import styles from './ScreenReaderOnly.css';

type ScreenReaderOnlyProps = ComponentPropsWithoutRef<'span'>;

function ScreenReaderOnly({
  className = styles.screenReaderOnly,
  ...otherProps
}: ScreenReaderOnlyProps) {
  return <span className={className} {...otherProps} />;
}

export default ScreenReaderOnly;
