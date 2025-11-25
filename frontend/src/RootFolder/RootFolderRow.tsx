import React, { useCallback, useState } from 'react';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import { RootFolder, useDeleteRootFolder } from './useRootFolders';
import styles from './RootFolderRow.css';

type RootFolderRowProps = RootFolder;

function RootFolderRow(props: RootFolderRowProps) {
  const {
    id,
    path,
    accessible,
    isEmpty,
    freeSpace = 0,
    unmappedFolders = [],
  } = props;

  const isUnavailable = !accessible;
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const { deleteRootFolder } = useDeleteRootFolder(id);

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, [setIsDeleteModalOpen]);

  const onConfirmDelete = useCallback(() => {
    deleteRootFolder();
    setIsDeleteModalOpen(false);
  }, [deleteRootFolder]);

  return (
    <TableRow>
      <TableRowCell>
        <div className={styles.pathContainer}>
          {isUnavailable ? (
            path
          ) : (
            <Link className={styles.link} to={`/add/import/${id}`}>
              {path}
            </Link>
          )}

          {isUnavailable ? (
            <Label className={styles.label} kind={kinds.DANGER}>
              {translate('Unavailable')}
            </Label>
          ) : null}

          {accessible && isEmpty ? (
            <Label
              className={styles.label}
              kind={kinds.WARNING}
              title={translate('EmptyRootFolderTooltip')}
            >
              {translate('Empty')}
            </Label>
          ) : null}
        </div>
      </TableRowCell>

      <TableRowCell className={styles.freeSpace}>
        {isUnavailable || isNaN(Number(freeSpace))
          ? '-'
          : formatBytes(freeSpace)}
      </TableRowCell>

      <TableRowCell className={styles.unmappedFolders}>
        {isUnavailable ? '-' : unmappedFolders.length}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          title={translate('RemoveRootFolder')}
          name={icons.REMOVE}
          onPress={onDeletePress}
        />
      </TableRowCell>

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('RemoveRootFolder')}
        message={translate('RemoveRootFolderWithSeriesMessageText', { path })}
        confirmLabel={translate('Remove')}
        onConfirm={onConfirmDelete}
        onCancel={onDeleteModalClose}
      />
    </TableRow>
  );
}

export default RootFolderRow;
