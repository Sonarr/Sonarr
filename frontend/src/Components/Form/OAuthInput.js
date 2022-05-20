import PropTypes from 'prop-types';
import React from 'react';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import { kinds } from 'Helpers/Props';

function OAuthInput(props) {
  const {
    label,
    authorizing,
    error,
    onPress
  } = props;

  return (
    <div>
      <SpinnerErrorButton
        kind={kinds.PRIMARY}
        isSpinning={authorizing}
        error={error}
        onPress={onPress}
      >
        {label}
      </SpinnerErrorButton>
    </div>
  );
}

OAuthInput.propTypes = {
  label: PropTypes.string.isRequired,
  authorizing: PropTypes.bool.isRequired,
  error: PropTypes.object,
  onPress: PropTypes.func.isRequired
};

OAuthInput.defaultProps = {
  label: 'Start OAuth'
};

export default OAuthInput;
