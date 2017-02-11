import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import SpinnerButton from 'Components/Link/SpinnerButton';

function OAuthInput(props) {
  const {
    authorizing,
    onPress
  } = props;

  return (
    <div>
      <SpinnerButton
        kind={kinds.PRIMARY}
        isSpinning={authorizing}
        onPress={onPress}
      >
        Start OAuth
      </SpinnerButton>
    </div>
  );
}

OAuthInput.propTypes = {
  authorizing: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default OAuthInput;
