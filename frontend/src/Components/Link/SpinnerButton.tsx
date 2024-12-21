import classNames from 'classnames';
import React from 'react';
import Icon, { IconName } from 'Components/Icon';
import { icons } from 'Helpers/Props';
import Button, { ButtonProps } from './Button';
import styles from './SpinnerButton.css';

export interface SpinnerButtonProps extends ButtonProps {
  isSpinning: boolean;
  isDisabled?: boolean;
  spinnerIcon?: IconName;
}

function SpinnerButton({
  className = styles.button,
  isSpinning,
  isDisabled,
  spinnerIcon = icons.SPINNER,
  children,
  ...otherProps
}: SpinnerButtonProps) {
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
        <Icon className={styles.spinner} name={spinnerIcon} isSpinning={true} />
      </span>

      <span className={styles.label}>{children}</span>
    </Button>
  );
}

export default SpinnerButton;
