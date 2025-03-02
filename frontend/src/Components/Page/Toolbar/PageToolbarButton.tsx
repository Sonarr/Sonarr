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
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  overflowComponent?: React.ComponentType<any>;
}

function PageToolbarButton({
  label,
  iconName,
  spinningName = icons.SPINNER,
  isDisabled = false,
  isSpinning = false,
  overflowComponent,
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
        size={21}
      />

      <div className={styles.labelContainer}>
        <div className={styles.label}>{label}</div>
      </div>
    </Link>
  );
}

export default PageToolbarButton;
