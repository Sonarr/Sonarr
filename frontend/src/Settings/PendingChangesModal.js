import PropTypes from 'prop-types';
import React, { useEffect } from 'react';
import keyboardShortcuts from 'Components/keyboardShortcuts';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';

function PendingChangesModal(props) {
  const {
    isOpen,
    onConfirm,
    onCancel,
    bindShortcut
  } = props;

  useEffect(() => {
    bindShortcut('enter', onConfirm);
  }, [bindShortcut, onConfirm]);

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onCancel}
    >
      <ModalContent onModalClose={onCancel}>
        <ModalHeader>Unsaved Changes</ModalHeader>

        <ModalBody>
          You have unsaved changes, are you sure you want to leave this page?
        </ModalBody>

        <ModalFooter>
          <Button
            kind={kinds.DEFAULT}
            onPress={onCancel}
          >
            Stay and review changes
          </Button>

          <Button
            autoFocus={true}
            kind={kinds.DANGER}
            onPress={onConfirm}
          >
            Discard changes and leave
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

PendingChangesModal.propTypes = {
  className: PropTypes.string,
  isOpen: PropTypes.bool.isRequired,
  kind: PropTypes.oneOf(kinds.all),
  onConfirm: PropTypes.func.isRequired,
  onCancel: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

PendingChangesModal.defaultProps = {
  kind: kinds.PRIMARY
};

export default keyboardShortcuts(PendingChangesModal);
