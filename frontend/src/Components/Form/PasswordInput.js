import PropTypes from 'prop-types';
import React from 'react';
import TextInput from './TextInput';
import styles from './PasswordInput.css';

function PasswordInput(props) {
  return (
    <TextInput
      {...props}
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
