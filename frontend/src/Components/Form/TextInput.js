import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import styles from './TextInput.css';

class TextInput extends Component {

  //
  // Listeners

  onChange = (event) => {
    this.props.onChange({
      name: this.props.name,
      value: event.target.value
    });
  }

  //
  // Render

  render() {
    const {
      className,
      type,
      readOnly,
      autoFocus,
      placeholder,
      name,
      value,
      hasError,
      hasWarning,
      hasButton,
      onFocus
    } = this.props;

    return (
      <input
        type={type}
        readOnly={readOnly}
        autoFocus={autoFocus}
        placeholder={placeholder}
        className={classNames(
          className,
          readOnly && styles.readOnly,
          hasError && styles.hasError,
          hasWarning && styles.hasWarning,
          hasButton && styles.hasButton
        )}
        name={name}
        value={value}
        onChange={this.onChange}
        onFocus={onFocus}
      />
    );
  }
}

TextInput.propTypes = {
  className: PropTypes.string.isRequired,
  type: PropTypes.string.isRequired,
  readOnly: PropTypes.bool,
  autoFocus: PropTypes.bool,
  placeholder: PropTypes.string,
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.array]).isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  hasButton: PropTypes.bool,
  onChange: PropTypes.func.isRequired,
  onFocus: PropTypes.func
};

TextInput.defaultProps = {
  className: styles.text,
  type: 'text',
  readOnly: false,
  autoFocus: false,
  value: ''
};

export default TextInput;
