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
import translate from 'Utilities/String/translate';

function PendingChangesModal(props) {
  const {
    isOpen,
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
      onModalClose={onCancel}
    >
      <ModalContent onModalClose={onCancel}>
        <ModalHeader>{translate('UnsavedChanges')}</ModalHeader>

        <ModalBody>
          {translate('PendingChangesMessage')}
        </ModalBody>

        <ModalFooter>
          <Button
            kind={kinds.DEFAULT}
            onPress={onCancel}
          >
            {translate('PendingChangesStayReview')}
          </Button>

          <Button
            autoFocus={true}
            kind={kinds.DANGER}
            onPress={onConfirm}
          >
            {translate('PendingChangesDiscardChanges')}
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
  bindShortcut: PropTypes.func.isRequired,
  unbindShortcut: PropTypes.func.isRequired
};

PendingChangesModal.defaultProps = {
  kind: kinds.PRIMARY
};

export default keyboardShortcuts(PendingChangesModal);
