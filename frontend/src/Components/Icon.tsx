import classNames from 'classnames';
import { LucideIcon, LucideProps } from 'lucide-react';
import React from 'react';
import { kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import styles from './Icon.css';

export type IconName = LucideIcon;
export type IconKind = Extract<Kind, keyof typeof styles>;

export interface IconProps {
  titleWrapperClassName?: string;
  className?: string;
  name: IconName;
  kind?: IconKind;
  size?: number;
  title?: string | (() => string) | null;
  isSpinning?: boolean;
  filled?: boolean;
}

export default function Icon({
  titleWrapperClassName,
  className,
  name: IconComponent,
  kind = kinds.DEFAULT,
  size = 14,
  title,
  isSpinning = false,
  filled = false,
}: IconProps) {
  const iconProps: LucideProps = {
    className: classNames(
      styles.icon,
      className,
      styles[kind],
      isSpinning && styles.spinning
    ),
    size,
    strokeWidth: 2,
    fill: filled ? 'currentColor' : 'none',
  };

  const icon = <IconComponent {...iconProps} />;

  if (title) {
    return (
      <span
        className={titleWrapperClassName}
        title={typeof title === 'function' ? title() : title}
      >
        {icon}
      </span>
    );
  }

  return icon;
}
