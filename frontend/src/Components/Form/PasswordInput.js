import React from 'react';
import TextInput from './TextInput';

function PasswordInput(props) {
  return (
    <TextInput
      type="password"
      {...props}
    />
  );
}

export default PasswordInput;
