import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageCustomFormatsEditModalContent from './ManageCustomFormatsEditModalContent';

interface ManageCustomFormatsEditModalProps {
  isOpen: boolean;
  customFormatIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function ManageCustomFormatsEditModal(
  props: ManageCustomFormatsEditModalProps
) {
  const { isOpen, customFormatIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageCustomFormatsEditModalContent
        customFormatIds={customFormatIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ManageCustomFormatsEditModal;
