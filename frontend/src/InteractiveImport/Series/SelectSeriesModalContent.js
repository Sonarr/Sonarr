import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import SelectSeriesRow from './SelectSeriesRow';
import styles from './SelectSeriesModalContent.css';

class SelectSeriesModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      filter: ''
    };
  }

  //
  // Listeners

  onFilterChange = ({ value }) => {
    this.setState({ filter: value });
  };

  //
  // Render

  render() {
    const {
      items,
      modalTitle,
      onSeriesSelect,
      onModalClose
    } = this.props;

    const filter = this.state.filter;
    const filterLower = filter.toLowerCase();

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {modalTitle} - Select Series
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <TextInput
            className={styles.filterInput}
            placeholder="Filter series"
            name="filter"
            value={filter}
            autoFocus={true}
            onChange={this.onFilterChange}
          />

          <Scroller
            className={styles.scroller}
            autoFocus={false}
          >
            {
              items.map((item) => {
                return item.title.toLowerCase().includes(filterLower) ?
                  (
                    <SelectSeriesRow
                      key={item.id}
                      id={item.id}
                      title={item.title}
                      onSeriesSelect={onSeriesSelect}
                    />
                  ) :
                  null;
              })
            }
          </Scroller>
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

SelectSeriesModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  modalTitle: PropTypes.string.isRequired,
  onSeriesSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectSeriesModalContent;
