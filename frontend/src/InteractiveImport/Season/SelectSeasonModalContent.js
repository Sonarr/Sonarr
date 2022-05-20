import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import SelectSeasonRow from './SelectSeasonRow';

class SelectSeasonModalContent extends Component {

  //
  // Render

  render() {
    const {
      items,
      modalTitle,
      onSeasonSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {modalTitle} - Select Season
        </ModalHeader>

        <ModalBody>
          {
            items.map((item) => {
              return (
                <SelectSeasonRow
                  key={item.seasonNumber}
                  seasonNumber={item.seasonNumber}
                  onSeasonSelect={onSeasonSelect}
                />
              );
            })
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectSeasonModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  modalTitle: PropTypes.string.isRequired,
  onSeasonSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectSeasonModalContent;
