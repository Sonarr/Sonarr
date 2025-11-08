import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import BackupRow from './BackupRow';
import RestoreBackupModal from './RestoreBackupModal';
import useBackups from './useBackups';

const columns: Column[] = [
  {
    name: 'type',
    label: '',
    isVisible: true,
  },
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isVisible: true,
  },
  {
    name: 'time',
    label: () => translate('Time'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

function Backups() {
  const dispatch = useDispatch();
  const { data: items, isLoading: isFetching, error, refetch } = useBackups();

  const isBackupExecuting = useSelector(
    createCommandExecutingSelector(commandNames.BACKUP)
  );

  const [isRestoreModalOpen, setIsRestoreModalOpen] = useState(false);

  const wasBackupExecuting = usePrevious(isBackupExecuting);
  const hasBackups = !!items.length;
  const noBackups = !items.length && !isFetching && !error;

  const handleBackupPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.BACKUP,
      })
    );
  }, [dispatch]);

  const handleRestorePress = useCallback(() => {
    setIsRestoreModalOpen(true);
  }, []);

  const handleRestoreModalClose = useCallback(() => {
    setIsRestoreModalOpen(false);
  }, []);

  useEffect(() => {
    if (wasBackupExecuting && !isBackupExecuting) {
      refetch();
    }
  }, [isBackupExecuting, wasBackupExecuting, refetch]);

  return (
    <PageContent title={translate('Backups')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('BackupNow')}
            iconName={icons.BACKUP}
            isSpinning={isBackupExecuting}
            onPress={handleBackupPress}
          />

          <PageToolbarButton
            label={translate('RestoreBackup')}
            iconName={icons.RESTORE}
            onPress={handleRestorePress}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('BackupsLoadError')}</Alert>
        ) : null}

        {noBackups ? (
          <Alert kind={kinds.INFO}>{translate('NoBackupsAreAvailable')}</Alert>
        ) : null}

        {hasBackups ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((item) => {
                const { id, type, name, path, size, time } = item;

                return (
                  <BackupRow
                    key={id}
                    id={id}
                    type={type}
                    name={name}
                    path={path}
                    size={size}
                    time={time}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}
      </PageContentBody>

      <RestoreBackupModal
        isOpen={isRestoreModalOpen}
        onModalClose={handleRestoreModalClose}
      />
    </PageContent>
  );
}

export default Backups;
