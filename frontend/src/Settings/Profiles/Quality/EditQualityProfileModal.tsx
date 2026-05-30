import React, { useCallback, useState } from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditQualityProfileModalContent from './EditQualityProfileModalContent';

interface EditQualityProfileModalProps {
  id?: number;
  cloneId?: number;
  isOpen: boolean;
  onDeleteQualityProfilePress?: () => void;
  onModalClose: () => void;
}

function EditQualityProfileModal({
  id,
  cloneId,
  isOpen,
  onDeleteQualityProfilePress,
  onModalClose,
}: EditQualityProfileModalProps) {
  const [height, setHeight] = useState<'auto' | number>('auto');

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
      onModalClose={onModalClose}
    >
      <EditQualityProfileModalContent
        id={id}
        cloneId={cloneId}
        onContentHeightChange={handleContentHeightChange}
        onDeleteQualityProfilePress={onDeleteQualityProfilePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditQualityProfileModal;
