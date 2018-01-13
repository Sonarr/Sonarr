import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { filterBuilderTypes, filterBuilderValueTypes, icons } from 'Helpers/Props';
import SelectInput from 'Components/Form/SelectInput';
import IconButton from 'Components/Link/IconButton';
import BoolFilterBuilderRowValue from './BoolFilterBuilderRowValue';
import DateFilterBuilderRowValue from './DateFilterBuilderRowValue';
import FilterBuilderRowValueConnector from './FilterBuilderRowValueConnector';
import IndexerFilterBuilderRowValueConnector from './IndexerFilterBuilderRowValueConnector';
import LanguageProfileFilterBuilderRowValueConnector from './LanguageProfileFilterBuilderRowValueConnector';
import ProtocolFilterBuilderRowValue from './ProtocolFilterBuilderRowValue';
import QualityFilterBuilderRowValueConnector from './QualityFilterBuilderRowValueConnector';
import QualityProfileFilterBuilderRowValueConnector from './QualityProfileFilterBuilderRowValueConnector';
import SeriesStatusFilterBuilderRowValue from './SeriesStatusFilterBuilderRowValue';
import TagFilterBuilderRowValueConnector from './TagFilterBuilderRowValueConnector';
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

function getDefaultFilterValue(selectedFilterBuilderProp) {
  if (selectedFilterBuilderProp.type === filterBuilderTypes.DATE) {
    return '';
  }

  return [];
}

function getRowValueConnector(selectedFilterBuilderProp) {
  if (!selectedFilterBuilderProp) {
    return FilterBuilderRowValueConnector;
  }

  const valueType = selectedFilterBuilderProp.valueType;

  switch (valueType) {
    case filterBuilderValueTypes.BOOL:
      return BoolFilterBuilderRowValue;

    case filterBuilderValueTypes.DATE:
      return DateFilterBuilderRowValue;

    case filterBuilderValueTypes.INDEXER:
      return IndexerFilterBuilderRowValueConnector;

    case filterBuilderValueTypes.LANGUAGE_PROFILE:
      return LanguageProfileFilterBuilderRowValueConnector;

    case filterBuilderValueTypes.PROTOCOL:
      return ProtocolFilterBuilderRowValue;

    case filterBuilderValueTypes.QUALITY:
      return QualityFilterBuilderRowValueConnector;

    case filterBuilderValueTypes.QUALITY_PROFILE:
      return QualityProfileFilterBuilderRowValueConnector;

    case filterBuilderValueTypes.SERIES_STATUS:
      return SeriesStatusFilterBuilderRowValue;

    case filterBuilderValueTypes.TAG:
      return TagFilterBuilderRowValueConnector;

    default:
      return FilterBuilderRowValueConnector;
  }
}

class FilterBuilderRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      filterKey,
      filterBuilderProps
    } = props;

    if (filterKey) {
      const selectedFilterBuilderProp = filterBuilderProps.find((a) => a.name === filterKey);
      this.selectedFilterBuilderProp = selectedFilterBuilderProp;
    }
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
      this.selectedFilterBuilderProp = selectedFilterBuilderProp;

      return;
    }

    const selectedFilterBuilderProp = filterBuilderProps[0];

    const filter = {
      key: selectedFilterBuilderProp.name,
      value: getDefaultFilterValue(selectedFilterBuilderProp),
      type: getDefaultFilterType(selectedFilterBuilderProp)
    };

    this.selectedFilterBuilderProp = selectedFilterBuilderProp;
    onFilterChange(index, filter);
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
      value: getDefaultFilterValue(selectedFilterBuilderProp),
      type
    };

    this.selectedFilterBuilderProp = selectedFilterBuilderProp;
    onFilterChange(index, filter);
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
      filterBuilderProps,
      sectionItems
    } = this.props;

    const selectedFilterBuilderProp = this.selectedFilterBuilderProp;

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
                filterType={filterType}
                filterValue={filterValue}
                selectedFilterBuilderProp={selectedFilterBuilderProp}
                sectionItems={sectionItems}
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
  filterValue: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.array, PropTypes.object]),
  filterType: PropTypes.string,
  filterCount: PropTypes.number.isRequired,
  filterBuilderProps: PropTypes.arrayOf(PropTypes.object).isRequired,
  sectionItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  onFilterChange: PropTypes.func.isRequired,
  onAddPress: PropTypes.func.isRequired,
  onRemovePress: PropTypes.func.isRequired
};

export default FilterBuilderRow;
