import React, { useCallback } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './ManageCustomFormatsModalRow.css';

interface ManageCustomFormatsModalRowProps {
  id: number;
  name: string;
  includeCustomFormatWhenRenaming: boolean;
  columns: Column[];
  isSelected?: boolean;
  onSelectedChange(result: SelectStateInputProps): void;
}

function ManageCustomFormatsModalRow(props: ManageCustomFormatsModalRowProps) {
  const {
    id,
    isSelected,
    name,
    includeCustomFormatWhenRenaming,
    onSelectedChange,
  } = props;

  const onSelectedChangeWrapper = useCallback(
    (result: SelectStateInputProps) => {
      onSelectedChange({
        ...result,
      });
    },
    [onSelectedChange]
  );

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.includeCustomFormatWhenRenaming}>
        {includeCustomFormatWhenRenaming ? translate('Yes') : translate('No')}
      </TableRowCell>
    </TableRow>
  );
}

export default ManageCustomFormatsModalRow;
