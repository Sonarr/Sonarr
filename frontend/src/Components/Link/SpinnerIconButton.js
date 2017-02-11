import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import IconButton from './IconButton';

function SpinnerIconButton(props) {
  const {
    name,
    spinningName,
    isDisabled,
    isSpinning,
    ...otherProps
  } = props;

  return (
    <IconButton
      name={isSpinning ? `${spinningName || name} fa-spin` : name}
      isDisabled={isDisabled || isSpinning}
      {...otherProps}
    />
  );
}

SpinnerIconButton.propTypes = {
  name: PropTypes.string.isRequired,
  spinningName: PropTypes.string.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  isSpinning: PropTypes.bool.isRequired
};

SpinnerIconButton.defaultProps = {
  spinningName: icons.SPINNER,
  isDisabled: false,
  isSpinning: false
};

export default SpinnerIconButton;
