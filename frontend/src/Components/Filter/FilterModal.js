import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import FilterBuilderModalContentConnector from './Builder/FilterBuilderModalContentConnector';
import CustomFiltersModalContent from './CustomFilters/CustomFiltersModalContent';

class FilterModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      filterBuilder: !props.customFilters.length,
      customFilterKey: null
    };
  }

  //
  // Listeners

  onAddCustomFilter = () => {
    this.setState({
      filterBuilder: true
    });
  }

  onEditCustomFilter = (customFilterKey) => {
    this.setState({
      filterBuilder: true,
      customFilterKey
    });
  }

  onModalClose = () => {
    this.setState({
      filterBuilder: false,
      customFilterKey: null
    }, () => {
      this.props.onModalClose();
    });
  }

  //
  // Render

  render() {
    const {
      isOpen,
      ...otherProps
    } = this.props;

    const {
      filterBuilder,
      customFilterKey
    } = this.state;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={this.onModalClose}
      >
        {
          filterBuilder ?
            <FilterBuilderModalContentConnector
              {...otherProps}
              customFilterKey={customFilterKey}
              onModalClose={this.onModalClose}
            /> :
            <CustomFiltersModalContent
              {...otherProps}
              onAddCustomFilter={this.onAddCustomFilter}
              onEditCustomFilter={this.onEditCustomFilter}
              onModalClose={this.onModalClose}
            />
        }
      </Modal>
    );
  }
}

FilterModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FilterModal;
