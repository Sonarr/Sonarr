import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import HistoryDetails from './HistoryDetails';
import styles from './HistoryDetailsModal.css';

function getHeaderTitle(eventType) {
  switch (eventType) {
    case 'grabbed':
      return 'Grabbed';
    case 'downloadFailed':
      return 'Download Failed';
    case 'downloadFolderImported':
      return 'Episode Imported';
    case 'episodeFileDeleted':
      return 'Episode File Deleted';
    case 'episodeFileRenamed':
      return 'Episode File Renamed';
    default:
      return 'Unknown';
  }
}

function HistoryDetailsModal(props) {
  const {
    isOpen,
    eventType,
    sourceTitle,
    data,
    isMarkingAsFailed,
    shortDateFormat,
    timeFormat,
    onMarkAsFailedPress,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {getHeaderTitle(eventType)}
        </ModalHeader>

        <ModalBody>
          <HistoryDetails
            eventType={eventType}
            sourceTitle={sourceTitle}
            data={data}
            shortDateFormat={shortDateFormat}
            timeFormat={timeFormat}
          />
        </ModalBody>

        <ModalFooter>
          {
            eventType === 'grabbed' &&
              <SpinnerButton
                className={styles.markAsFailedButton}
                kind={kinds.DANGER}
                isSpinning={isMarkingAsFailed}
                onPress={onMarkAsFailedPress}
              >
                Mark as Failed
              </SpinnerButton>
          }

          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

HistoryDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  isMarkingAsFailed: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

HistoryDetailsModal.defaultProps = {
  isMarkingAsFailed: false
};

export default HistoryDetailsModal;
