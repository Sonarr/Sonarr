import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditRemotePathMappingModalContent, {
  EditRemotePathMappingModalContentProps,
} from './EditRemotePathMappingModalContent';

interface EditRemotePathMappingModalProps
  extends EditRemotePathMappingModalContentProps {
  isOpen: boolean;
}

function EditRemotePathMappingModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditRemotePathMappingModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditRemotePathMappingModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditRemotePathMappingModal;
