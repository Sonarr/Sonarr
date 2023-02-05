import PropTypes from 'prop-types';
import React from 'react';
import styles from './InteractiveImportRowCellPlaceholder.css';

function InteractiveImportRowCellPlaceholder({ className }) {
  return (
    <span className={className} />
  );
}

InteractiveImportRowCellPlaceholder.propTypes = {
  className: PropTypes.string.isRequired
};

InteractiveImportRowCellPlaceholder.defaultProps = {
  className: styles.placeholder
};

export default InteractiveImportRowCellPlaceholder;
