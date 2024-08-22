import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditImportListExclusionModalContent from './EditImportListExclusionModalContent';

interface EditImportListExclusionModalProps {
  id?: number;
  isOpen: boolean;
  onModalClose: () => void;
  onDeleteImportListExclusionPress?: () => void;
}

function EditImportListExclusionModal(
  props: EditImportListExclusionModalProps
) {
  const { isOpen, onModalClose, ...otherProps } = props;

  const dispatch = useDispatch();

  const onModalClosePress = useCallback(() => {
    dispatch(
      clearPendingChanges({
        section: 'settings.importListExclusions',
      })
    );
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClosePress}>
      <EditImportListExclusionModalContent
        {...otherProps}
        onModalClose={onModalClosePress}
      />
    </Modal>
  );
}

export default EditImportListExclusionModal;
