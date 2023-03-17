import classNames from 'classnames';
import React from 'react';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './SeriesTypeSelectInputOption.css';

interface SeriesTypeSelectInputOptionProps {
  key: string;
  value: string;
  format: string;
  isMobile: boolean;
}

function SeriesTypeSelectInputOption(props: SeriesTypeSelectInputOptionProps) {
  const { key, value, format, isMobile, ...otherProps } = props;

  return (
    <EnhancedSelectInputOption id={key} isMobile={isMobile} {...otherProps}>
      <div
        className={classNames(styles.optionText, isMobile && styles.isMobile)}
      >
        <div className={styles.value}>{value}</div>

        {format == null ? null : <div className={styles.format}>{format}</div>}
      </div>
    </EnhancedSelectInputOption>
  );
}

export default SeriesTypeSelectInputOption;
