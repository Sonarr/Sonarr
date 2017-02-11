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

  //
  // Render

  render() {
    const {
      ...otherProps
    } = this.props;

    return (
      <TextInput
        type="number"
        {...otherProps}
        onChange={this.onChange}
      />
    );
  }
}

NumberInput.propTypes = {
  value: PropTypes.number,
  isFloat: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

NumberInput.defaultProps = {
  value: null,
  isFloat: false
};

export default NumberInput;
