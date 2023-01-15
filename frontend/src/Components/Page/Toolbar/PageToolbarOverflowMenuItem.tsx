import { IconDefinition } from '@fortawesome/fontawesome-common-types';
import React from 'react';
import MenuItem from 'Components/Menu/MenuItem';
import SpinnerIcon from 'Components/SpinnerIcon';
import styles from './PageToolbarOverflowMenuItem.css';

interface PageToolbarOverflowMenuItemProps {
  iconName: IconDefinition;
  spinningName?: IconDefinition;
  isDisabled?: boolean;
  isSpinning?: boolean;
  showIndicator?: boolean;
  label: string;
  text?: string;
  onPress: () => void;
}

function PageToolbarOverflowMenuItem(props: PageToolbarOverflowMenuItemProps) {
  const {
    iconName,
    spinningName,
    label,
    isDisabled,
    isSpinning = false,
    ...otherProps
  } = props;

  return (
    <MenuItem key={label} isDisabled={isDisabled || isSpinning} {...otherProps}>
      <SpinnerIcon
        className={styles.icon}
        name={iconName}
        spinningName={spinningName}
        isSpinning={isSpinning}
      />
      {label}
    </MenuItem>
  );
}

export default PageToolbarOverflowMenuItem;
