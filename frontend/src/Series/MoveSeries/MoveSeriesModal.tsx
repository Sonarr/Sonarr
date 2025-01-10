import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MoveSeriesModal.css';

interface MoveSeriesModalProps {
  originalPath?: string;
  destinationPath?: string;
  destinationRootFolder?: string;
  isOpen: boolean;
  onModalClose: () => void;
  onSavePress: () => void;
  onMoveSeriesPress: () => void;
}

function MoveSeriesModal({
  originalPath,
  destinationPath,
  destinationRootFolder,
  isOpen,
  onModalClose,
  onSavePress,
  onMoveSeriesPress,
}: MoveSeriesModalProps) {
  if (isOpen && !originalPath && !destinationPath && !destinationRootFolder) {
    console.error(
      'originalPath and destinationPath OR destinationRootFolder must be provided'
    );
  }

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <ModalContent showCloseButton={true} onModalClose={onModalClose}>
        <ModalHeader>{translate('MoveFiles')}</ModalHeader>

        <ModalBody>
          {destinationRootFolder
            ? translate('MoveSeriesFoldersToRootFolder', {
                destinationRootFolder,
              })
            : null}

          {originalPath && destinationPath
            ? translate('MoveSeriesFoldersToNewPath', {
                originalPath,
                destinationPath,
              })
            : null}
        </ModalBody>

        <ModalFooter>
          <Button className={styles.doNotMoveButton} onPress={onSavePress}>
            {translate('MoveSeriesFoldersDontMoveFiles')}
          </Button>

          <Button kind={kinds.DANGER} onPress={onMoveSeriesPress}>
            {translate('MoveSeriesFoldersMoveFiles')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default MoveSeriesModal;
