import React from 'react';
import { icons } from 'Helpers/Props';
import Icon, { IconName, IconProps } from './Icon';

export interface SpinnerIconProps extends IconProps {
  spinningName?: IconName;
  isSpinning: Required<IconProps['isSpinning']>;
}

export default function SpinnerIcon({
  name,
  spinningName = icons.SPINNER,
  isSpinning,
  ...otherProps
}: SpinnerIconProps) {
  return (
    <Icon
      name={(isSpinning && spinningName) || name}
      isSpinning={isSpinning}
      {...otherProps}
    />
  );
}
