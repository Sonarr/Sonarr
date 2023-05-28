import React, { useCallback } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import TagListConnector from 'Components/TagListConnector';
import { SelectStateInputProps } from 'typings/props';
import styles from './ManageIndexersModalRow.css';

interface ManageIndexersModalRowProps {
  id: number;
  name: string;
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  priority: number;
  implementation: string;
  tags: number[];
  columns: Column[];
  isSelected?: boolean;
  onSelectedChange(result: SelectStateInputProps): void;
}

function ManageIndexersModalRow(props: ManageIndexersModalRowProps) {
  const {
    id,
    isSelected,
    name,
    enableRss,
    enableAutomaticSearch,
    enableInteractiveSearch,
    priority,
    implementation,
    tags,
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

      <TableRowCell className={styles.enableRss}>
        {enableRss ? 'Yes' : 'No'}
      </TableRowCell>

      <TableRowCell className={styles.enableAutomaticSearch}>
        {enableAutomaticSearch ? 'Yes' : 'No'}
      </TableRowCell>

      <TableRowCell className={styles.enableInteractiveSearch}>
        {enableInteractiveSearch ? 'Yes' : 'No'}
      </TableRowCell>

      <TableRowCell className={styles.priority}>{priority}</TableRowCell>

      <TableRowCell className={styles.tags}>
        <TagListConnector tags={tags} />
      </TableRowCell>
    </TableRow>
  );
}

export default ManageIndexersModalRow;
