import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import EditMetadataModalContent, {
  EditMetadataModalContentProps,
} from './EditMetadataModalContent';

interface EditMetadataModalProps
  extends Omit<EditMetadataModalContentProps, 'advancedSettings'> {
  isOpen: boolean;
}

function EditMetadataModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMetadataModalProps) {
  const advancedSettings = useShowAdvancedSettings();

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditMetadataModalContent
        {...otherProps}
        advancedSettings={advancedSettings}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditMetadataModal;
