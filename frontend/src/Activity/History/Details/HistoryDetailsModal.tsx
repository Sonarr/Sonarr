import React, { useCallback, useEffect, useRef } from 'react';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { HistoryData, HistoryEventType } from 'typings/History';
import translate from 'Utilities/String/translate';
import { useMarkAsFailed } from '../useHistory';
import HistoryDetails from './HistoryDetails';
import styles from './HistoryDetailsModal.module.css';

function getHeaderTitle(eventType: HistoryEventType) {
  switch (eventType) {
    case 'grabbed':
      return translate('Grabbed');
    case 'downloadFailed':
      return translate('DownloadFailed');
    case 'downloadFolderImported':
      return translate('EpisodeImported');
    case 'episodeFileDeleted':
      return translate('EpisodeFileDeleted');
    case 'episodeFileRenamed':
      return translate('EpisodeFileRenamed');
    case 'downloadIgnored':
      return translate('DownloadIgnored');
    default:
      return translate('Unknown');
  }
}

interface HistoryDetailsModalProps {
  isOpen: boolean;
  id: number;
  eventType: HistoryEventType;
  sourceTitle: string;
  data: HistoryData;
  downloadId?: string;
  onModalClose: () => void;
}

function HistoryDetailsModal(props: HistoryDetailsModalProps) {
  const { isOpen, id, eventType, sourceTitle, data, downloadId, onModalClose } =
    props;

  const { markAsFailed, isMarkingAsFailed, markAsFailedError } =
    useMarkAsFailed(id);

  const wasMarkingAsFailed = useRef(isMarkingAsFailed);

  const handleMarkAsFailedPress = useCallback(() => {
    markAsFailed();
  }, [markAsFailed]);

  useEffect(() => {
    if (wasMarkingAsFailed && !isMarkingAsFailed && !markAsFailedError) {
      onModalClose();
    }
  }, [wasMarkingAsFailed, isMarkingAsFailed, markAsFailedError, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>{getHeaderTitle(eventType)}</ModalHeader>

        <ModalBody>
          <HistoryDetails
            eventType={eventType}
            sourceTitle={sourceTitle}
            data={data}
            downloadId={downloadId}
          />
        </ModalBody>

        <ModalFooter>
          {eventType === 'grabbed' && (
            <SpinnerButton
              className={styles.markAsFailedButton}
              kind={kinds.DANGER}
              isSpinning={isMarkingAsFailed}
              onPress={handleMarkAsFailedPress}
            >
              {translate('MarkAsFailed')}
            </SpinnerButton>
          )}

          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default HistoryDetailsModal;
