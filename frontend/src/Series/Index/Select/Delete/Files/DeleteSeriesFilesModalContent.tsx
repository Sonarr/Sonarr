import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import SeriesDeleteList from '../SeriesDeleteList';
import useSelectedSeriesStats from '../useSelectedSeriesStats';
import styles from './DeleteSeriesFilesModalContent.css';

export interface DeleteSeriesFilesModalContentProps {
  onModalClose(): void;
}

function DeleteSeriesFilesModalContent({
  onModalClose,
}: DeleteSeriesFilesModalContentProps) {
  const { series, seriesIds, totalEpisodeFileCount, totalSizeOnDisk } =
    useSelectedSeriesStats();
  const executeCommand = useExecuteCommand();

  const onDeleteSeriesConfirmed = useCallback(() => {
    executeCommand({
      name: CommandNames.DeleteSeriesFiles,
      seriesIds,
    });

    onModalClose();
  }, [seriesIds, executeCommand, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedSeriesFiles')}</ModalHeader>

      <ModalBody>
        <div className={styles.message}>
          {translate('DeleteSeriesFilesConfirmation', {
            count: series.length,
          })}
        </div>

        <SeriesDeleteList
          series={series}
          showFileDetails={true}
          totalEpisodeFileCount={totalEpisodeFileCount}
          totalSizeOnDisk={totalSizeOnDisk}
          styles={styles}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteSeriesConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteSeriesFilesModalContent;
