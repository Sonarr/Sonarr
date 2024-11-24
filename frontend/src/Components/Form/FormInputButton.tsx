import classNames from 'classnames';
import React from 'react';
import Button, { ButtonProps } from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import { kinds } from 'Helpers/Props';
import styles from './FormInputButton.css';

export interface FormInputButtonProps extends ButtonProps {
  canSpin?: boolean;
  isLastButton?: boolean;
}

function FormInputButton({
  className = styles.button,
  canSpin = false,
  isLastButton = true,
  kind = kinds.PRIMARY,
  ...otherProps
}: FormInputButtonProps) {
  if (canSpin) {
    return (
      <SpinnerButton
        className={classNames(className, !isLastButton && styles.middleButton)}
        kind={kind}
        {...otherProps}
      />
    );
  }

  return (
    <Button
      className={classNames(className, !isLastButton && styles.middleButton)}
      kind={kind}
      {...otherProps}
    />
  );
}

export default FormInputButton;
