import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import FilterBuilderModalContentConnector from './Builder/FilterBuilderModalContentConnector';
import CustomFiltersModalContentConnector from './CustomFilters/CustomFiltersModalContentConnector';

class FilterModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      filterBuilder: !props.customFilters.length,
      id: null
    };
  }

  //
  // Listeners

  onAddCustomFilter = () => {
    this.setState({
      filterBuilder: true
    });
  };

  onEditCustomFilter = (id) => {
    this.setState({
      filterBuilder: true,
      id
    });
  };

  onCancelPress = () => {
    if (this.state.filterBuilder) {
      this.setState({
        filterBuilder: false,
        id: null
      });
    } else {
      this.onModalClose();
    }
  };

  onModalClose = () => {
    this.setState({
      filterBuilder: false,
      id: null
    }, () => {
      this.props.onModalClose();
    });
  };

  //
  // Render

  render() {
    const {
      isOpen,
      ...otherProps
    } = this.props;

    const {
      filterBuilder,
      id
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
              id={id}
              onCancelPress={this.onCancelPress}
              onModalClose={this.onModalClose}
            /> :
            <CustomFiltersModalContentConnector
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
