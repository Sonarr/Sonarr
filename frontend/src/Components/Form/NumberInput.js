import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from './TextInput';

class NumberInput extends Component {

  //
  // Listeners

  onChange = ({ name, value }) => {
    let newValue = null;

    if (value) {
      newValue = this.props.isFloat ? parseFloat(value) : parseInt(value);
    }

    this.props.onChange({
      name,
      value: newValue
    });
  }

  onBlur = () => {
    const {
      name,
      value,
      min,
      max,
      onChange
    } = this.props;

    let newValue = value;

    if (min != null && newValue != null && newValue < min) {
      newValue = min;
    } else if (max != null && newValue != null && newValue > max) {
      newValue = max;
    }

    onChange({
      name,
      value: newValue
    });
  }

  //
  // Render

  render() {
    const {
      value,
      ...otherProps
    } = this.props;

    return (
      <TextInput
        type="number"
        value={value == null ? '' : value}
        {...otherProps}
        onChange={this.onChange}
        onBlur={this.onBlur}
      />
    );
  }
}

NumberInput.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.number,
  min: PropTypes.number,
  max: PropTypes.number,
  isFloat: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

NumberInput.defaultProps = {
  value: null,
  isFloat: false
};

export default NumberInput;
