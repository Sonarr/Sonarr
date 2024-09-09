import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import { RootFolderSelectInputValue } from './RootFolderSelectInput';
import styles from './RootFolderSelectInputSelectedValue.css';

interface RootFolderSelectInputSelectedValueProps {
  selectedValue: string;
  values: RootFolderSelectInputValue[];
  freeSpace?: number;
  seriesFolder?: string;
  isWindows?: boolean;
  includeFreeSpace?: boolean;
}

function RootFolderSelectInputSelectedValue(
  props: RootFolderSelectInputSelectedValueProps
) {
  const {
    selectedValue,
    values,
    freeSpace,
    seriesFolder,
    includeFreeSpace = true,
    isWindows,
    ...otherProps
  } = props;

  const slashCharacter = isWindows ? '\\' : '/';
  const value = values.find((v) => v.key === selectedValue)?.value;

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.pathContainer}>
        <div className={styles.path}>{value}</div>

        {seriesFolder ? (
          <div className={styles.seriesFolder}>
            {slashCharacter}
            {seriesFolder}
          </div>
        ) : null}
      </div>

      {freeSpace != null && includeFreeSpace ? (
        <div className={styles.freeSpace}>
          {translate('RootFolderSelectFreeSpace', {
            freeSpace: formatBytes(freeSpace),
          })}
        </div>
      ) : null}
    </EnhancedSelectInputSelectedValue>
  );
}

export default RootFolderSelectInputSelectedValue;
