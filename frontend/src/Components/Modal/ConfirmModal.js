import PropTypes from 'prop-types';
import React, { useEffect } from 'react';
import keyboardShortcuts from 'Components/keyboardShortcuts';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds, sizes } from 'Helpers/Props';

function ConfirmModal(props) {
  const {
    isOpen,
    kind,
    size,
    title,
    message,
    confirmLabel,
    cancelLabel,
    hideCancelButton,
    isSpinning,
    onConfirm,
    onCancel,
    bindShortcut,
    unbindShortcut
  } = props;

  useEffect(() => {
    if (isOpen) {
      bindShortcut('enter', onConfirm);

      return () => unbindShortcut('enter', onConfirm);
    }
  }, [bindShortcut, unbindShortcut, isOpen, onConfirm]);

  return (
    <Modal
      isOpen={isOpen}
      size={size}
      onModalClose={onCancel}
    >
      <ModalContent onModalClose={onCancel}>
        <ModalHeader>{title}</ModalHeader>

        <ModalBody>
          {message}
        </ModalBody>

        <ModalFooter>
          {
            !hideCancelButton &&
              <Button
                kind={kinds.DEFAULT}
                onPress={onCancel}
              >
                {cancelLabel}
              </Button>
          }

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

ConfirmModal.propTypes = {
  className: PropTypes.string,
  isOpen: PropTypes.bool.isRequired,
  kind: PropTypes.oneOf(kinds.all),
  size: PropTypes.oneOf(sizes.all),
  title: PropTypes.string.isRequired,
  message: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired,
  confirmLabel: PropTypes.string,
  cancelLabel: PropTypes.string,
  hideCancelButton: PropTypes.bool,
  isSpinning: PropTypes.bool.isRequired,
  onConfirm: PropTypes.func.isRequired,
  onCancel: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired,
  unbindShortcut: PropTypes.func.isRequired
};

ConfirmModal.defaultProps = {
  kind: kinds.PRIMARY,
  size: sizes.MEDIUM,
  confirmLabel: 'OK',
  cancelLabel: 'Cancel',
  isSpinning: false
};

export default keyboardShortcuts(ConfirmModal);
