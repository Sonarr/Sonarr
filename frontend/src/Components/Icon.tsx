import {
  FontAwesomeIcon,
  FontAwesomeIconProps,
} from '@fortawesome/react-fontawesome';
import classNames from 'classnames';
import React, { ComponentProps } from 'react';
import { kinds } from 'Helpers/Props';
import styles from './Icon.css';

export interface IconProps
  extends Omit<
    FontAwesomeIconProps,
    'icon' | 'spin' | 'name' | 'title' | 'size'
  > {
  containerClassName?: ComponentProps<'span'>['className'];
  name: FontAwesomeIconProps['icon'];
  kind?: Extract<(typeof kinds.all)[number], keyof typeof styles>;
  size?: number;
  isSpinning?: FontAwesomeIconProps['spin'];
  title?: string | (() => string);
}

export default function Icon({
  containerClassName,
  className,
  name,
  kind = kinds.DEFAULT,
  size = 14,
  title,
  isSpinning = false,
  fixedWidth = false,
  ...otherProps
}: IconProps) {
  const icon = (
    <FontAwesomeIcon
      className={classNames(className, styles[kind])}
      icon={name}
      spin={isSpinning}
      fixedWidth={fixedWidth}
      style={{
        fontSize: `${size}px`,
      }}
      {...otherProps}
    />
  );

  if (title) {
    return (
      <span
        className={containerClassName}
        title={typeof title === 'function' ? title() : title}
      >
        {icon}
      </span>
    );
  }

  return icon;
}
