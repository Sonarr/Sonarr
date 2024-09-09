import classNames from 'classnames';
import React from 'react';
import EnhancedSelectInputOption, {
  EnhancedSelectInputOptionProps,
} from './EnhancedSelectInputOption';
import styles from './SeriesTypeSelectInputOption.css';

interface SeriesTypeSelectInputOptionProps
  extends EnhancedSelectInputOptionProps {
  id: string;
  value: string;
  format: string;
  isMobile: boolean;
}

function SeriesTypeSelectInputOption(props: SeriesTypeSelectInputOptionProps) {
  const { id, value, format, isMobile, ...otherProps } = props;

  return (
    <EnhancedSelectInputOption {...otherProps} id={id} isMobile={isMobile}>
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
