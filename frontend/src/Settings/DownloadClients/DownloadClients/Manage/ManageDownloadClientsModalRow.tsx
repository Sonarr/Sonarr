import React, { useCallback } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { SelectStateInputProps } from 'typings/props';
import styles from './ManageDownloadClientsModalRow.css';

interface ManageDownloadClientsModalRowProps {
  id: number;
  name: string;
  enable: boolean;
  priority: number;
  removeCompletedDownloads: boolean;
  removeFailedDownloads: boolean;
  implementation: string;
  columns: Column[];
  isSelected?: boolean;
  onSelectedChange(result: SelectStateInputProps): void;
}

function ManageDownloadClientsModalRow(
  props: ManageDownloadClientsModalRowProps
) {
  const {
    id,
    isSelected,
    name,
    enable,
    priority,
    removeCompletedDownloads,
    removeFailedDownloads,
    implementation,
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

      <TableRowCell className={styles.implementation}>
        {implementation}
      </TableRowCell>

      <TableRowCell className={styles.enable}>
        {enable ? 'Yes' : 'No'}
      </TableRowCell>

      <TableRowCell className={styles.priority}>{priority}</TableRowCell>

      <TableRowCell className={styles.removeCompletedDownloads}>
        {removeCompletedDownloads ? 'Yes' : 'No'}
      </TableRowCell>

      <TableRowCell className={styles.removeFailedDownloads}>
        {removeFailedDownloads ? 'Yes' : 'No'}
      </TableRowCell>
    </TableRow>
  );
}

export default ManageDownloadClientsModalRow;
