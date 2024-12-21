import React, { useEffect } from 'react';
import Button from 'Components/Link/Button';
import SpinnerButton, {
  SpinnerButtonProps,
} from 'Components/Link/SpinnerButton';
import Modal, { ModalProps } from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';

interface ConfirmModalProps extends Omit<ModalProps, 'onModalClose'> {
  kind?: SpinnerButtonProps['kind'];
  title: string;
  message: React.ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  hideCancelButton?: boolean;
  isSpinning?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

function ConfirmModal({
  isOpen,
  kind = 'primary',
  size = 'medium',
  title,
  message,
  confirmLabel = 'OK',
  cancelLabel = 'Cancel',
  hideCancelButton,
  isSpinning = false,
  onConfirm,
  onCancel,
}: ConfirmModalProps) {
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  useEffect(() => {
    if (isOpen) {
      bindShortcut('acceptConfirmModal', onConfirm);
    }

    return () => unbindShortcut('acceptConfirmModal');
  }, [bindShortcut, unbindShortcut, isOpen, onConfirm]);

  return (
    <Modal isOpen={isOpen} size={size} onModalClose={onCancel}>
      <ModalContent onModalClose={onCancel}>
        <ModalHeader>{title}</ModalHeader>

        <ModalBody>{message}</ModalBody>

        <ModalFooter>
          {!hideCancelButton && (
            <Button kind="default" onPress={onCancel}>
              {cancelLabel}
            </Button>
          )}

          <SpinnerButton
            autoFocus={true}
            kind={kind}
            isSpinning={isSpinning}
            onPress={onConfirm}
          >
            {confirmLabel}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default ConfirmModal;
