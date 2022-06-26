import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import Button from './Button';
import styles from './SpinnerButton.css';

function SpinnerButton(props) {
  const {
    className,
    isSpinning,
    isDisabled,
    spinnerIcon,
    children,
    ...otherProps
  } = props;

  return (
    <Button
      className={classNames(
        className,
        styles.button,
        isSpinning && styles.isSpinning
      )}
      isDisabled={isDisabled || isSpinning}
      {...otherProps}
    >
      <span className={styles.spinnerContainer}>
        <Icon
          className={styles.spinner}
          name={spinnerIcon}
          isSpinning={true}
        />
      </span>

      <span className={styles.label}>
        {children}
      </span>
    </Button>
  );
}

SpinnerButton.propTypes = {
  className: PropTypes.string.isRequired,
  isSpinning: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool,
  spinnerIcon: PropTypes.object.isRequired,
  children: PropTypes.node
};

SpinnerButton.defaultProps = {
  className: styles.button,
  spinnerIcon: icons.SPINNER
};

export default SpinnerButton;
