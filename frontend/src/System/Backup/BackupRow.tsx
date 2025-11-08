import React, { useCallback, useMemo, useState } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import { BackupType } from 'typings/Backup';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import RestoreBackupModal from './RestoreBackupModal';
import { useDeleteBackup } from './useBackups';
import styles from './BackupRow.css';

interface BackupRowProps {
  id: number;
  type: BackupType;
  name: string;
  path: string;
  size: number;
  time: string;
}

function BackupRow({ id, type, name, path, size, time }: BackupRowProps) {
  const deleteBackupMutation = useDeleteBackup(id);
  const [isRestoreModalOpen, setIsRestoreModalOpen] = useState(false);
  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);

  const { iconClassName, iconTooltip } = useMemo(() => {
    if (type === 'manual') {
      return {
        iconClassName: icons.INTERACTIVE,
        iconTooltip: translate('Manual'),
      };
    }

    if (type === 'update') {
      return {
        iconClassName: icons.UPDATE,
        iconTooltip: translate('BeforeUpdate'),
      };
    }

    return {
      iconClassName: icons.SCHEDULED,
      iconTooltip: translate('Scheduled'),
    };
  }, [type]);

  const handleRestorePress = useCallback(() => {
    setIsRestoreModalOpen(true);
  }, []);

  const handleRestoreModalClose = useCallback(() => {
    setIsRestoreModalOpen(false);
  }, []);

  const handleDeletePress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, []);

  const handleConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, []);

  const handleConfirmDeletePress = useCallback(() => {
    deleteBackupMutation.mutate(undefined, {
      onSuccess: () => {
        setIsConfirmDeleteModalOpen(false);
      },
      onError: (error) => {
        console.error('Failed to delete backup:', error);
      },
    });
  }, [deleteBackupMutation]);

  return (
    <TableRow key={id}>
      <TableRowCell className={styles.type}>
        <Icon name={iconClassName} title={iconTooltip} />
      </TableRowCell>

      <TableRowCell>
        <Link to={`${window.Sonarr.urlBase}${path}`} noRouter={true}>
          {name}
        </Link>
      </TableRowCell>

      <TableRowCell>{formatBytes(size)}</TableRowCell>

      <RelativeDateCell date={time} />

      <TableRowCell className={styles.actions}>
        <IconButton
          title={translate('RestoreBackup')}
          name={icons.RESTORE}
          onPress={handleRestorePress}
        />

        <IconButton
          title={translate('DeleteBackup')}
          name={icons.DELETE}
          onPress={handleDeletePress}
        />
      </TableRowCell>

      <RestoreBackupModal
        isOpen={isRestoreModalOpen}
        id={id}
        name={name}
        onModalClose={handleRestoreModalClose}
      />

      <ConfirmModal
        isOpen={isConfirmDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteBackup')}
        message={translate('DeleteBackupMessageText', {
          name,
        })}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeletePress}
        onCancel={handleConfirmDeleteModalClose}
      />
    </TableRow>
  );
}

export default BackupRow;
