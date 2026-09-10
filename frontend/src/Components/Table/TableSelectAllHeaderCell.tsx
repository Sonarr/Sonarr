import React, { useMemo } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import TableHeaderCell from './TableHeaderCell';
import styles from './TableSelectAllHeaderCell.module.css';

interface TableSelectAllHeaderCellProps {
  allSelected: boolean;
  allUnselected: boolean;
  onSelectAllChange: (change: CheckInputChanged) => void;
}

function TableSelectAllHeaderCell({
  allSelected,
  allUnselected,
  onSelectAllChange,
}: TableSelectAllHeaderCellProps) {
  const value = useMemo(() => {
    if (allSelected) {
      return true;
    } else if (allUnselected) {
      return false;
    }

    return null;
  }, [allSelected, allUnselected]);

  return (
    <TableHeaderCell className={styles.selectAllHeaderCell} name="selectAll">
      <CheckInput
        className={styles.input}
        containerClassName={styles.container}
        name="selectAll"
        ariaLabel={translate('SelectAll')}
        value={value}
        onChange={onSelectAllChange}
      />
    </TableHeaderCell>
  );
}

export default TableSelectAllHeaderCell;
