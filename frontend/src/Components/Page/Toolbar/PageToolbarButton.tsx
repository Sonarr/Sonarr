import classNames from 'classnames';
import React from 'react';
import Icon, { IconName } from 'Components/Icon';
import Link, { LinkProps } from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './PageToolbarButton.css';

export interface PageToolbarButtonProps extends LinkProps {
  label: string;
  iconName: IconName;
  spinningName?: IconName;
  isSpinning?: boolean;
  isDisabled?: boolean;
}

function PageToolbarButton({
  label,
  iconName,
  spinningName = icons.SPINNER,
  isDisabled = false,
  isSpinning = false,
  ...otherProps
}: PageToolbarButtonProps) {
  return (
    <Link
      className={classNames(
        styles.toolbarButton,
        isDisabled && styles.isDisabled
      )}
      isDisabled={isDisabled || isSpinning}
      title={label}
      {...otherProps}
    >
      <Icon
        name={isSpinning ? spinningName || iconName : iconName}
        isSpinning={isSpinning}
        size={16}
      />

      <span className={styles.label}>{label}</span>
    </Link>
  );
}

export default PageToolbarButton;
