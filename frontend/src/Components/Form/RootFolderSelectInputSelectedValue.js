import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import styles from './RootFolderSelectInputSelectedValue.css';

function RootFolderSelectInputSelectedValue(props) {
  const {
    value,
    freeSpace,
    includeFreeSpace,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.path}>
        {value}
      </div>

      {
        freeSpace != null && includeFreeSpace &&
          <div className={styles.freeSpace}>
            {formatBytes(freeSpace)} Free
          </div>
      }
    </EnhancedSelectInputSelectedValue>
  );
}

RootFolderSelectInputSelectedValue.propTypes = {
  value: PropTypes.string,
  freeSpace: PropTypes.number,
  includeFreeSpace: PropTypes.bool.isRequired
};

RootFolderSelectInputSelectedValue.defaultProps = {
  includeFreeSpace: true
};

export default RootFolderSelectInputSelectedValue;
