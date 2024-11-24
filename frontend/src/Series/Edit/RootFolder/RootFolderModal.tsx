import React from 'react';
import Modal from 'Components/Modal/Modal';
import RootFolderModalContent, {
  RootFolderModalContentProps,
} from './RootFolderModalContent';

interface RootFolderModalProps extends RootFolderModalContentProps {
  isOpen: boolean;
}

function RootFolderModal(props: RootFolderModalProps) {
  const { isOpen, rootFolderPath, seriesId, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <RootFolderModalContent
        seriesId={seriesId}
        rootFolderPath={rootFolderPath}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default RootFolderModal;
