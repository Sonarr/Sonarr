import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearOrganizePreview } from 'Store/Actions/organizePreviewActions';
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
  const dispatch = useDispatch();

  const handleOnModalClose = useCallback(() => {
    dispatch(clearOrganizePreview());
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleOnModalClose}>
      {isOpen ? (
        <OrganizePreviewModalContent
          {...otherProps}
          onModalClose={handleOnModalClose}
        />
      ) : null}
    </Modal>
  );
}
export default OrganizePreviewModal;
