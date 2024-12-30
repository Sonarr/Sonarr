import React, { useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditQualityProfileModalContent from './EditQualityProfileModalContent';

interface EditQualityProfileModalProps {
  id?: number;
  isOpen: boolean;
  onDeleteQualityProfilePress?: () => void;
  onModalClose: () => void;
}

function EditQualityProfileModal({
  id,
  isOpen,
  onDeleteQualityProfilePress,
  onModalClose,
}: EditQualityProfileModalProps) {
  const dispatch = useDispatch();
  const [height, setHeight] = useState<'auto' | number>('auto');

  const handleOnModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.qualityProfiles' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  const handleContentHeightChange = useCallback(
    (newHeight: number) => {
      if (height === 'auto' || newHeight !== 0) {
        setHeight(newHeight);
      }
    },
    [height]
  );

  return (
    <Modal
      style={{ height: height === 'auto' ? 'auto' : `${height}px` }}
      isOpen={isOpen}
      size={sizes.EXTRA_LARGE}
      onModalClose={handleOnModalClose}
    >
      <EditQualityProfileModalContent
        id={id}
        onContentHeightChange={handleContentHeightChange}
        onDeleteQualityProfilePress={onDeleteQualityProfilePress}
        onModalClose={handleOnModalClose}
      />
    </Modal>
  );
}

export default EditQualityProfileModal;
