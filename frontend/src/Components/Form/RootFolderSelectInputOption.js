import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './RootFolderSelectInputOption.css';

function RootFolderSelectInputOption(props) {
  const {
    id,
    value,
    freeSpace,
    isMissing,
    seriesFolder,
    isMobile,
    isWindows,
    ...otherProps
  } = props;

  const slashCharacter = isWindows ? '\\' : '/';

  return (
    <EnhancedSelectInputOption
      id={id}
      isMobile={isMobile}
      {...otherProps}
    >
      <div className={classNames(
        styles.optionText,
        isMobile && styles.isMobile
      )}
      >
        <div className={styles.value}>
          {value}

          {
            seriesFolder && id !== 'addNew' ?
              <div className={styles.seriesFolder}>
                {slashCharacter}
                {seriesFolder}
              </div> :
              null
          }
        </div>

        {
          freeSpace == null ?
            null :
            <div className={styles.freeSpace}>
              {formatBytes(freeSpace)} Free
            </div>
        }

        {
          isMissing ?
            <div className={styles.isMissing}>
              Missing
            </div> :
            null
        }
      </div>
    </EnhancedSelectInputOption>
  );
}

RootFolderSelectInputOption.propTypes = {
  id: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  freeSpace: PropTypes.number,
  isMissing: PropTypes.boolean,
  seriesFolder: PropTypes.string,
  isMobile: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool
};

export default RootFolderSelectInputOption;
