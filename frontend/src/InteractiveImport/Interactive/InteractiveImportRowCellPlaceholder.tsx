import classNames from 'classnames';
import React from 'react';
import styles from './InteractiveImportRowCellPlaceholder.module.css';

interface InteractiveImportRowCellPlaceholderProps {
  isOptional?: boolean;
}

function InteractiveImportRowCellPlaceholder(
  props: InteractiveImportRowCellPlaceholderProps
) {
  return (
    <span
      className={classNames(
        styles.placeholder,
        props.isOptional && styles.optional
      )}
    />
  );
}

export default InteractiveImportRowCellPlaceholder;
