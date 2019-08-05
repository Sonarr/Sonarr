import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

class RemoveQueueItemModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      remove: true,
      blacklist: false
    };
  }

  //
  // Control

  resetState = function() {
    this.setState({
      remove: true,
      blacklist: false
    });
  }

  //
  // Listeners

  onRemoveChange = ({ value }) => {
    this.setState({ remove: value });
  }

  onBlacklistChange = ({ value }) => {
    this.setState({ blacklist: value });
  }

  onRemoveConfirmed = () => {
    const state = this.state;

    this.resetState();
    this.props.onRemovePress(state);
  }

  onModalClose = () => {
    this.resetState();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      isOpen,
      sourceTitle,
      canIgnore
    } = this.props;

    const { remove, blacklist } = this.state;

    return (
      <Modal
        isOpen={isOpen}
        size={sizes.MEDIUM}
        onModalClose={this.onModalClose}
      >
        <ModalContent
          onModalClose={this.onModalClose}
        >
          <ModalHeader>
            Remove - {sourceTitle}
          </ModalHeader>

          <ModalBody>
            <div>
              Are you sure you want to remove '{sourceTitle}' from the queue?
            </div>

            <FormGroup>
              <FormLabel>Remove From Download Client</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="remove"
                value={remove}
                helpTextWarning="Removing will remove the download and the file(s) from the download client."
                isDisabled={!canIgnore}
                onChange={this.onRemoveChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Blacklist Release</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="blacklist"
                value={blacklist}
                helpText="Starts a search for this episode again and prevents this release from being grabbed again"
                onChange={this.onBlacklistChange}
              />
            </FormGroup>

          </ModalBody>

          <ModalFooter>
            <Button onPress={this.onModalClose}>
              Close
            </Button>

            <Button
              kind={kinds.DANGER}
              onPress={this.onRemoveConfirmed}
            >
              Remove
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

RemoveQueueItemModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  canIgnore: PropTypes.bool.isRequired,
  onRemovePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RemoveQueueItemModal;
