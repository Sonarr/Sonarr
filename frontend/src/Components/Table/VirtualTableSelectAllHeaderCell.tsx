import React, { useMemo } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import { CheckInputChanged } from 'typings/inputs';
import VirtualTableHeaderCell from './VirtualTableHeaderCell';
import styles from './VirtualTableSelectAllHeaderCell.css';

interface VirtualTableSelectAllHeaderCellProps {
  allSelected: boolean;
  allUnselected: boolean;
  onSelectAllChange: (change: CheckInputChanged) => void;
}

function VirtualTableSelectAllHeaderCell({
  allSelected,
  allUnselected,
  onSelectAllChange,
}: VirtualTableSelectAllHeaderCellProps) {
  const value = useMemo(() => {
    if (allSelected) {
      return true;
    } else if (allUnselected) {
      return false;
    }

    return null;
  }, [allSelected, allUnselected]);

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

export default VirtualTableSelectAllHeaderCell;
