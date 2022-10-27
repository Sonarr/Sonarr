import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSpecificationModalContent from './EditSpecificationModalContent';

function EditSpecificationModal({ isOpen, onModalClose, ...otherProps }) {
  const dispatch = useDispatch();

  const onWrappedModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.autoTaggingSpecifications' }));
    onModalClose();
  }, [onModalClose, dispatch]);

  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditSpecificationModalContent
        {...otherProps}
        onModalClose={onWrappedModalClose}
      />
    </Modal>
  );
}

EditSpecificationModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditSpecificationModal;
