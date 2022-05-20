import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import styles from './RemoveQueueItemsModal.css';

class RemoveQueueItemsModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      remove: true,
      blocklist: false
    };
  }

  //
  // Control

  resetState = function() {
    this.setState({
      remove: true,
      blocklist: false
    });
  };

  //
  // Listeners

  onRemoveChange = ({ value }) => {
    this.setState({ remove: value });
  };

  onBlocklistChange = ({ value }) => {
    this.setState({ blocklist: value });
  };

  onRemoveConfirmed = () => {
    const state = this.state;

    this.resetState();
    this.props.onRemovePress(state);
  };

  onModalClose = () => {
    this.resetState();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      isOpen,
      selectedCount,
      canIgnore,
      allPending
    } = this.props;

    const { remove, blocklist } = this.state;

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
            Remove Selected Item{selectedCount > 1 ? 's' : ''}
          </ModalHeader>

          <ModalBody>
            <div className={styles.message}>
              Are you sure you want to remove {selectedCount} item{selectedCount > 1 ? 's' : ''} from the queue?
            </div>

            {
              allPending ?
                null :
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
            }

            <FormGroup>
              <FormLabel>
                Add Release{selectedCount > 1 ? 's' : ''} To Blocklist
              </FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="blocklist"
                value={blocklist}
                helpText="Prevents Sonarr from automatically grabbing this episode again"
                onChange={this.onBlocklistChange}
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

RemoveQueueItemsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  selectedCount: PropTypes.number.isRequired,
  canIgnore: PropTypes.bool.isRequired,
  allPending: PropTypes.bool.isRequired,
  onRemovePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RemoveQueueItemsModal;
