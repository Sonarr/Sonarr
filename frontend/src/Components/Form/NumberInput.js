import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from './TextInput';

function parseValue(props, value) {
  const {
    isFloat,
    min,
    max
  } = props;

  if (value == null || value === '') {
    return null;
  }

  let newValue = isFloat ? parseFloat(value) : parseInt(value);

  if (min != null && newValue != null && newValue < min) {
    newValue = min;
  } else if (max != null && newValue != null && newValue > max) {
    newValue = max;
  }

  return newValue;
}

class NumberInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      value: props.value == null ? '' : props.value.toString(),
      isFocused: false
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const { value } = this.props;

    if (!isNaN(value) && value !== prevProps.value && !this.state.isFocused) {
      this.setState({
        value: value == null ? '' : value.toString()
      });
    }
  }

  //
  // Listeners

  onChange = ({ name, value }) => {
    this.setState({ value });

    this.props.onChange({
      name,
      value: parseValue(this.props, value)
    });

  };

  onFocus = () => {
    this.setState({ isFocused: true });
  };

  onBlur = () => {
    const {
      name,
      onChange
    } = this.props;

    const { value } = this.state;
    const parsedValue = parseValue(this.props, value);
    const stringValue = parsedValue == null ? '' : parsedValue.toString();

    if (stringValue === value) {
      this.setState({ isFocused: false });
    } else {
      this.setState({
        value: stringValue,
        isFocused: false
      });
    }

    onChange({
      name,
      value: parsedValue
    });
  };

  //
  // Render

  render() {
    const value = this.state.value;

    return (
      <TextInput
        {...this.props}
        type="number"
        value={value == null ? '' : value}
        onChange={this.onChange}
        onBlur={this.onBlur}
        onFocus={this.onFocus}
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
