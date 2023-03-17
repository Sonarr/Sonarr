import React from 'react';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import styles from './SeriesTypeSelectInputSelectedValue.css';

interface SeriesTypeSelectInputOptionProps {
  key: string;
  value: string;
  format: string;
}
function SeriesTypeSelectInputSelectedValue(
  props: SeriesTypeSelectInputOptionProps
) {
  const { value, format, ...otherProps } = props;

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.value}>{value}</div>

      {format == null ? null : <div className={styles.format}>{format}</div>}
    </EnhancedSelectInputSelectedValue>
  );
}

export default SeriesTypeSelectInputSelectedValue;
