import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import Label from 'Components/Label';
import SeriesTagList from 'Components/SeriesTagList';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { kinds } from 'Helpers/Props';
import DownloadClient from 'typings/DownloadClient';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './ManageDownloadClientsModalRow.css';

interface ManageDownloadClientsModalRowProps {
  id: number;
  name: string;
  enable: boolean;
  priority: number;
  removeCompletedDownloads: boolean;
  removeFailedDownloads: boolean;
  implementation: string;
  tags: number[];
  columns: Column[];
}

function ManageDownloadClientsModalRow(
  props: ManageDownloadClientsModalRowProps
) {
  const {
    id,
    name,
    enable,
    priority,
    removeCompletedDownloads,
    removeFailedDownloads,
    implementation,
    tags,
  } = props;

  const { toggleSelected, useIsSelected } = useSelect<DownloadClient>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.implementation}>
        {implementation}
      </TableRowCell>

      <TableRowCell className={styles.enable}>
        <Label kind={enable ? kinds.SUCCESS : kinds.DISABLED} outline={!enable}>
          {enable ? translate('Yes') : translate('No')}
        </Label>
      </TableRowCell>

      <TableRowCell className={styles.priority}>{priority}</TableRowCell>

      <TableRowCell className={styles.removeCompletedDownloads}>
        {removeCompletedDownloads ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.removeFailedDownloads}>
        {removeFailedDownloads ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.tags}>
        <SeriesTagList tags={tags} />
      </TableRowCell>
    </TableRow>
  );
}

export default ManageDownloadClientsModalRow;
