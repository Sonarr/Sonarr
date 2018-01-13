import PropTypes from 'prop-types';
import React from 'react';
import CheckInput from 'Components/Form/CheckInput';
import VirtualTableHeaderCell from './VirtualTableHeaderCell';
import styles from './VirtualTableSelectAllHeaderCell.css';

function getValue(allSelected, allUnselected) {
  if (allSelected) {
    return true;
  } else if (allUnselected) {
    return false;
  }

  return null;
}

function VirtualTableSelectAllHeaderCell(props) {
  const {
    allSelected,
    allUnselected,
    onSelectAllChange
  } = props;

  const value = getValue(allSelected, allUnselected);

  return (
    <VirtualTableHeaderCell
      className={styles.selectAllHeaderCell}
      name="selectAll"
    >
      <CheckInput
        className={styles.input}
        name="selectAll"
        value={value}
        onChange={onSelectAllChange}
      />
    </VirtualTableHeaderCell>
  );
}

VirtualTableSelectAllHeaderCell.propTypes = {
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  onSelectAllChange: PropTypes.func.isRequired
};

export default VirtualTableSelectAllHeaderCell;
