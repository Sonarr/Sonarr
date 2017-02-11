import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from './Icon';

function SpinnerIcon(props) {
  const {
    name,
    spinningName,
    isSpinning,
    ...otherProps
  } = props;

  return (
    <Icon
      name={isSpinning ? `${spinningName || name} fa-spin` : name}
      {...otherProps}
    />
  );
}

SpinnerIcon.propTypes = {
  name: PropTypes.string.isRequired,
  spinningName: PropTypes.string.isRequired,
  isSpinning: PropTypes.bool.isRequired
};

SpinnerIcon.defaultProps = {
  spinningName: icons.SPINNER
};

export default SpinnerIcon;
