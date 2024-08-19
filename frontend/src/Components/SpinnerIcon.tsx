import React from 'react';
import { icons } from 'Helpers/Props';
import Icon, { IconProps } from './Icon';

export interface SpinnerIconProps extends IconProps {
  spinningName?: IconProps['name'];
  isSpinning: Required<IconProps['isSpinning']>;
}

export default function SpinnerIcon({
  name,
  spinningName = icons.SPINNER,
  ...otherProps
}: SpinnerIconProps) {
  return (
    <Icon
      name={(otherProps.isSpinning && spinningName) || name}
      {...otherProps}
    />
  );
}
