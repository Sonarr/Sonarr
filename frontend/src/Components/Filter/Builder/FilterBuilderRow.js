import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { filterBuilderTypes, filterBuilderValueTypes, icons } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import IconButton from 'Components/Link/IconButton';
import FilterBuilderRowValueConnector from './FilterBuilderRowValueConnector';
import IndexerFilterBuilderRowValueConnector from './IndexerFilterBuilderRowValueConnector';
import ProtocolFilterBuilderRowValue from './ProtocolFilterBuilderRowValue';
import QualityFilterBuilderRowValueConnector from './QualityFilterBuilderRowValueConnector';
import styles from './FilterBuilderRow.css';

function getselectedFilterBuilderProp(filterBuilderProps, name) {
  return filterBuilderProps.find((a) => {
    return a.name === name;
  });
}

function getFilterTypeOptions(filterBuilderProps, filterKey) {
  const selectedFilterBuilderProp = getselectedFilterBuilderProp(filterBuilderProps, filterKey);

  if (!selectedFilterBuilderProp) {
    return [];
  }

  return filterBuilderTypes.possibleFilterTypes[selectedFilterBuilderProp.type];
}

function getDefaultFilterType(selectedFilterBuilderProp) {
  return filterBuilderTypes.possibleFilterTypes[selectedFilterBuilderProp.type][0].key;
}

function getRowValueConnector(selectedFilterBuilderProp) {
  if (!selectedFilterBuilderProp) {
    return FilterBuilderRowValueConnector;
  }

  const valueType = selectedFilterBuilderProp.valueType;

  switch (valueType) {
    case filterBuilderValueTypes.INDEXER:
      return IndexerFilterBuilderRowValueConnector;

    case filterBuilderValueTypes.PROTOCOL:
      return ProtocolFilterBuilderRowValue;

    case filterBuilderValueTypes.QUALITY:
      return QualityFilterBuilderRowValueConnector;

    default:
      return FilterBuilderRowValueConnector;
  }
}

class FilterBuilderRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      selectedFilterBuilderProp: null
    };
  }

  componentDidMount() {
    const {
      index,
      filterKey,
      filterBuilderProps,
      onFilterChange
    } = this.props;

    if (filterKey) {
      const selectedFilterBuilderProp = filterBuilderProps.find((a) => a.name === filterKey);
      this.setState({ selectedFilterBuilderProp });

      return;
    }

    const selectedFilterBuilderProp = filterBuilderProps[0];

    const filter = {
      key: selectedFilterBuilderProp.name,
      value: [],
      type: getDefaultFilterType(selectedFilterBuilderProp)
    };

    this.setState({ selectedFilterBuilderProp }, () => {
      onFilterChange(index, filter);
    });
  }

  //
  // Listeners

  onFilterKeyChange = ({ value: key }) => {
    const {
      index,
      filterBuilderProps,
      onFilterChange
    } = this.props;

    const selectedFilterBuilderProp = getselectedFilterBuilderProp(filterBuilderProps, key);
    const type = getDefaultFilterType(selectedFilterBuilderProp);

    const filter = {
      key,
      value: [],
      type
    };

    this.setState({ selectedFilterBuilderProp }, () => {
      onFilterChange(index, filter);
    });
  }

  onFilterChange = ({ name, value }) => {
    const {
      index,
      filterKey,
      filterValue,
      filterType,
      onFilterChange
    } = this.props;

    const filter = {
      key: filterKey,
      value: filterValue,
      type: filterType
    };

    filter[name] = value;

    onFilterChange(index, filter);
  }

  onAddPress = () => {
    const {
      index,
      onAddPress
    } = this.props;

    onAddPress(index);
  }

  onRemovePress = () => {
    const {
      index,
      onRemovePress
    } = this.props;

    onRemovePress(index);
  }

  //
  // Render

  render() {
    const {
      filterKey,
      filterType,
      filterValue,
      filterCount,
      filterBuilderProps
    } = this.props;

    const {
      selectedFilterBuilderProp
    } = this.state;

    const keyOptions = filterBuilderProps.map((availablePropFilter) => {
      return {
        key: availablePropFilter.name,
        value: availablePropFilter.label
      };
    });

    const ValueComponent = getRowValueConnector(selectedFilterBuilderProp);

    return (
      <div className={styles.filterRow}>
        <div className={styles.inputContainer}>
          {
            filterKey &&
              <SelectInput
                name="key"
                value={filterKey}
                values={keyOptions}
                onChange={this.onFilterKeyChange}
              />
          }
        </div>

        <div className={styles.inputContainer}>
          {
            filterType &&
              <SelectInput
                name="type"
                value={filterType}
                values={getFilterTypeOptions(filterBuilderProps, filterKey)}
                onChange={this.onFilterChange}
              />
          }
        </div>

        <div className={styles.valueInputContainer}>
          {
            filterValue != null && !!selectedFilterBuilderProp &&
              <ValueComponent
                filterValue={filterValue}
                selectedFilterBuilderProp={selectedFilterBuilderProp}
                onChange={this.onFilterChange}
              />
          }
        </div>

        <div className={styles.actionsContainer}>
          <IconButton
            name={icons.SUBTRACT}
            isDisabled={filterCount === 1}
            onPress={this.onRemovePress}
          />

          <IconButton
            name={icons.ADD}
            onPress={this.onAddPress}
          />
        </div>
      </div>
    );
  }
}

FilterBuilderRow.propTypes = {
  index: PropTypes.number.isRequired,
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.array]),
  filterType: PropTypes.string,
  filterCount: PropTypes.number.isRequired,
  filterBuilderProps: PropTypes.arrayOf(PropTypes.object).isRequired,
  onFilterChange: PropTypes.func.isRequired,
  onAddPress: PropTypes.func.isRequired,
  onRemovePress: PropTypes.func.isRequired
};

export default FilterBuilderRow;
