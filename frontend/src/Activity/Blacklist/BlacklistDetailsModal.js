import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

class BlacklistDetailsModal extends Component {

  //
  // Render

  render() {
    const {
      isOpen,
      sourceTitle,
      protocol,
      indexer,
      message,
      onModalClose
    } = this.props;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <ModalContent
          onModalClose={onModalClose}
        >
          <ModalHeader>
            Details
          </ModalHeader>

          <ModalBody>
            <DescriptionList>
              <DescriptionListItem
                title="Name"
                data={sourceTitle}
              />

              <DescriptionListItem
                title="Protocol"
                data={protocol}
              />

              {
                !!message &&
                  <DescriptionListItem
                    title="Indexer"
                    data={indexer}
                  />
              }

              {
                !!message &&
                  <DescriptionListItem
                    title="Message"
                    data={message}
                  />
              }
            </DescriptionList>
          </ModalBody>

          <ModalFooter>
            <Button onPress={onModalClose}>
              Close
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

BlacklistDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  protocol: PropTypes.string.isRequired,
  indexer: PropTypes.string,
  message: PropTypes.string,
  onModalClose: PropTypes.func.isRequired
};

export default BlacklistDetailsModal;
