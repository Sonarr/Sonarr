import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import OrganizePreviewModalContent, {
  OrganizePreviewModalContentProps,
} from './OrganizePreviewModalContent';

interface OrganizePreviewModalProps extends OrganizePreviewModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function OrganizePreviewModal({
  isOpen,
  onModalClose,
  ...otherProps
}: OrganizePreviewModalProps) {
  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      onModalClose={onModalClose}
    >
      {isOpen ? (
        <OrganizePreviewModalContent
          {...otherProps}
          onModalClose={onModalClose}
        />
      ) : null}
    </Modal>
  );
}
export default OrganizePreviewModal;
