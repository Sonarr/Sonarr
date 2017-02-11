import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import formatBytes from 'Utilities/Number/formatBytes';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './RootFolderSelectInputOption.css';

function RootFolderSelectInputOption(props) {
  const {
    value,
    freeSpace,
    isMobile,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputOption
      isMobile={isMobile}
      {...otherProps}
    >
      <div className={classNames(
        styles.optionText,
        isMobile && styles.isMobile
      )}
      >
        <div>{value}</div>

        {
          freeSpace != null &&
            <div className={styles.freeSpace}>
              {formatBytes(freeSpace)} Free
            </div>
        }
      </div>
    </EnhancedSelectInputOption>
  );
}

RootFolderSelectInputOption.propTypes = {
  value: PropTypes.string.isRequired,
  freeSpace: PropTypes.number,
  isMobile: PropTypes.bool.isRequired
};

export default RootFolderSelectInputOption;
