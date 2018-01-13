import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import { kinds } from 'Helpers/Props';
import styles from './FormInputButton.css';

function FormInputButton(props) {
  const {
    className,
    canSpin,
    isLastButton,
    ...otherProps
  } = props;

  if (canSpin) {
    return (
      <SpinnerButton
        className={classNames(
          className,
          !isLastButton && styles.middleButton
        )}
        kind={kinds.PRIMARY}
        {...otherProps}
      />
    );
  }

  return (
    <Button
      className={classNames(
        className,
        !isLastButton && styles.middleButton
      )}
      kind={kinds.PRIMARY}
      {...otherProps}
    />
  );
}

FormInputButton.propTypes = {
  className: PropTypes.string.isRequired,
  isLastButton: PropTypes.bool.isRequired,
  canSpin: PropTypes.bool.isRequired
};

FormInputButton.defaultProps = {
  className: styles.button,
  isLastButton: true,
  canSpin: false
};

export default FormInputButton;
