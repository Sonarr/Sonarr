import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EpisodeDetailsModalContentConnector from './EpisodeDetailsModalContentConnector';

class EpisodeDetailsModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      closeOnBackgroundClick: props.selectedTab !== 'search'
    };
  }

  //
  // Listeners

  onTabChange = (isSearch) => {
    this.setState({ closeOnBackgroundClick: !isSearch });
  };

  //
  // Render

  render() {
    const {
      isOpen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <Modal
        isOpen={isOpen}
        size={sizes.EXTRA_LARGE}
        closeOnBackgroundClick={this.state.closeOnBackgroundClick}
        onModalClose={onModalClose}
      >
        <EpisodeDetailsModalContentConnector
          {...otherProps}
          onTabChange={this.onTabChange}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

EpisodeDetailsModal.propTypes = {
  selectedTab: PropTypes.string,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EpisodeDetailsModal;
