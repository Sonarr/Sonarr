import React from 'react';
import TextInput from './TextInput';

// Prevent a user from copying (or cutting) the password from the input
function onCopy(e) {
  e.preventDefault();
  e.nativeEvent.stopImmediatePropagation();
}

function PasswordInput(props) {
  return (
    <TextInput
      {...props}
      type="password"
      onCopy={onCopy}
    />
  );
}

PasswordInput.propTypes = {
  ...TextInput.props
};

export default PasswordInput;
