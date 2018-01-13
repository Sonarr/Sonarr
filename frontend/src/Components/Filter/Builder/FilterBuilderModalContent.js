import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import FilterBuilderRow from './FilterBuilderRow';
import styles from './FilterBuilderModalContent.css';

class FilterBuilderModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const filters = [...props.filters];

    // Push an empty filter if there aren't any filters. FilterBuilderRow
    // will handle initializing the filter.

    if (!filters.length) {
      filters.push({});
    }

    this.state = {
      label: props.label,
      filters,
      labelErrors: []
    };
  }

  //
  // Listeners

  onLabelChange = ({ value }) => {
    this.setState({ label: value });
  }

  onFilterChange = (index, filter) => {
    const filters = [...this.state.filters];
    filters.splice(index, 1, filter);

    this.setState({
      filters
    });
  }

  onAddFilterPress = () => {
    const filters = [...this.state.filters];
    filters.push({});

    this.setState({
      filters
    });
  }

  onRemoveFilterPress = (index) => {
    const filters = [...this.state.filters];
    filters.splice(index, 1);

    this.setState({
      filters
    });
  }

  onSaveFilterPress = () => {
    const {
      customFilterKey: key,
      onSaveCustomFilterPress,
      onModalClose
    } = this.props;

    const {
      label,
      filters
    } = this.state;

    if (!label) {
      this.setState({
        labelErrors: [
          {
            message: 'Label is required'
          }
        ]
      });

      return;
    }

    onSaveCustomFilterPress({ key, label, filters });
    onModalClose();
  }

  //
  // Render

  render() {
    const {
      sectionItems,
      filterBuilderProps,
      onModalClose
    } = this.props;

    const {
      label,
      filters,
      labelErrors
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Custom Filter
        </ModalHeader>

        <ModalBody>
          <div className={styles.labelContainer}>
            <div className={styles.label}>
              Label
            </div>

            <div className={styles.labelInputContainer}>
              <FormInputGroup
                name="label"
                value={label}
                type={inputTypes.TEXT}
                errors={labelErrors}
                onChange={this.onLabelChange}
              />
            </div>
          </div>

          <div className={styles.label}>Filters</div>

          <div className={styles.rows}>
            {
              filters.map((filter, index) => {
                return (
                  <FilterBuilderRow
                    key={index}
                    index={index}
                    sectionItems={sectionItems}
                    filterBuilderProps={filterBuilderProps}
                    filterKey={filter.key}
                    filterValue={filter.value}
                    filterType={filter.type}
                    filterCount={filters.length}
                    onAddPress={this.onAddFilterPress}
                    onRemovePress={this.onRemoveFilterPress}
                    onFilterChange={this.onFilterChange}
                  />
                );
              })
            }
          </div>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>

          <Button
            onPress={this.onSaveFilterPress}
          >
            Apply
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

FilterBuilderModalContent.propTypes = {
  customFilterKey: PropTypes.string,
  label: PropTypes.string.isRequired,
  sectionItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterBuilderProps: PropTypes.arrayOf(PropTypes.object).isRequired,
  onRemoveCustomFilterPress: PropTypes.func.isRequired,
  onSaveCustomFilterPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FilterBuilderModalContent;
