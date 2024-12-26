import React, { useEffect } from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface PendingChangesModalProps {
  className?: string;
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

function PendingChangesModal({
  isOpen,
  onConfirm,
  onCancel,
}: PendingChangesModalProps) {
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  useEffect(() => {
    if (isOpen) {
      bindShortcut('acceptConfirmModal', onConfirm);
    }

    return () => unbindShortcut('acceptConfirmModal');
  }, [bindShortcut, unbindShortcut, isOpen, onConfirm]);

  return (
    <Modal isOpen={isOpen} onModalClose={onCancel}>
      <ModalContent onModalClose={onCancel}>
        <ModalHeader>{translate('UnsavedChanges')}</ModalHeader>

        <ModalBody>{translate('PendingChangesMessage')}</ModalBody>

        <ModalFooter>
          <Button kind={kinds.DEFAULT} onPress={onCancel}>
            {translate('PendingChangesStayReview')}
          </Button>

          <Button autoFocus={true} kind={kinds.DANGER} onPress={onConfirm}>
            {translate('PendingChangesDiscardChanges')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default PendingChangesModal;
