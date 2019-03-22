import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

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
    onCancel
  } = props;

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
            data-autofocus={true}
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
  onCancel: PropTypes.func.isRequired
};

ConfirmModal.defaultProps = {
  kind: kinds.PRIMARY,
  size: sizes.MEDIUM,
  confirmLabel: 'OK',
  cancelLabel: 'Cancel',
  isSpinning: false
};

export default ConfirmModal;
