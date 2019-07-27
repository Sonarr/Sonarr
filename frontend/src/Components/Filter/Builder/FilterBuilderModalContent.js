import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
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

  componentDidUpdate(prevProps) {
    const {
      id,
      customFilters,
      isSaving,
      saveError,
      dispatchSetFilter,
      onModalClose
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      if (id) {
        dispatchSetFilter({ selectedFilterKey: id });
      } else {
        const last = customFilters[customFilters.length -1];
        dispatchSetFilter({ selectedFilterKey: last.id });
      }

      onModalClose();
    }
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
      id,
      customFilterType,
      onSaveCustomFilterPress
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

    onSaveCustomFilterPress({
      id,
      type: customFilterType,
      label,
      filters
    });
  }

  //
  // Render

  render() {
    const {
      sectionItems,
      filterBuilderProps,
      isSaving,
      saveError,
      onCancelPress,
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
          <Button onPress={onCancelPress}>
            Cancel
          </Button>

          <SpinnerErrorButton
            isSpinning={isSaving}
            error={saveError}
            onPress={this.onSaveFilterPress}
          >
            Save
          </SpinnerErrorButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

FilterBuilderModalContent.propTypes = {
  id: PropTypes.number,
  label: PropTypes.string.isRequired,
  customFilterType: PropTypes.string.isRequired,
  sectionItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterBuilderProps: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchDeleteCustomFilter: PropTypes.func.isRequired,
  onSaveCustomFilterPress: PropTypes.func.isRequired,
  dispatchSetFilter: PropTypes.func.isRequired,
  onCancelPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FilterBuilderModalContent;
