import PropTypes from 'prop-types';
import React from 'react';
import TextInput from './TextInput';
import styles from './PasswordInput.css';

// Prevent a user from copying (or cutting) the password from the input
function onCopy(e) {
  e.preventDefault();
  e.nativeEvent.stopImmediatePropagation();
}

function PasswordInput(props) {
  return (
    <TextInput
      {...props}
      onCopy={onCopy}
    />
  );
}

PasswordInput.propTypes = {
  className: PropTypes.string.isRequired
};

PasswordInput.defaultProps = {
  className: styles.input
};

export default PasswordInput;
