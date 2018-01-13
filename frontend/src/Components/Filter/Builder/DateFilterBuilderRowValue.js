import PropTypes from 'prop-types';
import React, { Component } from 'react';
import isString from 'Utilities/String/isString';
import { IN_LAST, IN_NEXT } from 'Helpers/Props/filterTypes';
import NumberInput from 'Components/Form/NumberInput';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import { NAME } from './FilterBuilderRowValue';
import styles from './DateFilterBuilderRowValue.css';

const timeOptions = [
  { key: 'seconds', value: 'seconds' },
  { key: 'minutes', value: 'minutes' },
  { key: 'hours', value: 'hours' },
  { key: 'days', value: 'days' },
  { key: 'weeks', value: 'weeks' },
  { key: 'months', value: 'months' }
];

function isInFilter(filterType) {
  return filterType === IN_LAST || filterType === IN_NEXT;
}

class DateFilterBuilderRowValue extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      filterType,
      filterValue,
      onChange
    } = this.props;

    if (isInFilter(filterType) && isString(filterValue)) {
      onChange({
        name: NAME,
        value: {
          time: timeOptions[0].key,
          value: null
        }
      });
    }
  }

  componentDidUpdate(prevProps) {
    const {
      filterType,
      filterValue,
      onChange
    } = this.props;

    if (prevProps.filterType === filterType) {
      return;
    }

    if (isInFilter(filterType) && isString(filterValue)) {
      onChange({
        name: NAME,
        value: {
          time: timeOptions[0].key,
          value: null
        }
      });

      return;
    }

    if (!isInFilter(filterType) && !isString(filterValue)) {
      onChange({
        name: NAME,
        value: ''
      });
    }
  }

  //
  // Listeners

  onValueChange = ({ value }) => {
    const {
      filterValue,
      onChange
    } = this.props;

    let newValue = value;

    if (!isString(value)) {
      newValue = {
        time: filterValue.time,
        value
      };
    }

    onChange({
      name: NAME,
      value: newValue
    });
  }

  onTimeChange = ({ value }) => {
    const {
      filterValue,
      onChange
    } = this.props;

    onChange({
      name: NAME,
      value: {
        time: value,
        value: filterValue.value
      }
    });
  }

  //
  // Render

  render() {
    const {
      filterType,
      filterValue
    } = this.props;

    if (
      (isInFilter(filterType) && isString(filterValue)) ||
      (!isInFilter(filterType) && !isString(filterValue))
    ) {
      return null;
    }

    if (isInFilter(filterType)) {
      return (
        <div className={styles.container}>
          <NumberInput
            className={styles.numberInput}
            name={NAME}
            value={filterValue.value}
            onChange={this.onValueChange}
          />

          <SelectInput
            className={styles.selectInput}
            name={NAME}
            value={filterValue.time}
            values={timeOptions}
            onChange={this.onTimeChange}
          />
        </div>
      );
    }

    return (
      <TextInput
        name={NAME}
        value={filterValue}
        placeholder="yyyy-mm-dd"
        onChange={this.onValueChange}
      />
    );
  }
}

DateFilterBuilderRowValue.propTypes = {
  filterType: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.string, PropTypes.object]).isRequired,
  onChange: PropTypes.func.isRequired
};

export default DateFilterBuilderRowValue;
