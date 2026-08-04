import classNames from 'classnames';
import React, {
  ComponentPropsWithoutRef,
  ElementType,
  SyntheticEvent,
  useCallback,
} from 'react';
import { Link as RouterLink } from 'react-router-dom';
import styles from './Link.css';

export type LinkProps<C extends ElementType = 'button'> =
  ComponentPropsWithoutRef<C> & {
    component?: C;
    to?: string;
    target?: string;
    isDisabled?: LinkProps<C>['disabled'];
    noRouter?: boolean;
    onPress?(event: SyntheticEvent): void;
  };

export default function Link<C extends ElementType = 'button'>({
  className,
  component,
  to,
  target,
  type,
  tabIndex,
  isDisabled,
  noRouter,
  onPress,
  ...otherProps
}: LinkProps<C>) {
  const Component = component || 'button';

  const onClick = useCallback(
    (event: SyntheticEvent) => {
      if (isDisabled) {
        event.preventDefault();
        event.stopPropagation();
        return;
      }

      onPress?.(event);
    },
    [isDisabled, onPress]
  );

  const linkClass = classNames(
    className,
    styles.link,
    to && styles.to,
    isDisabled && 'isDisabled'
  );

  if (to) {
    const toLink = /\w+?:\/\//.test(to);

    if (toLink || noRouter) {
      return (
        <a
          href={to}
          target={target || (toLink ? '_blank' : '_self')}
          rel={toLink ? 'noreferrer' : undefined}
          className={linkClass}
          aria-disabled={isDisabled || undefined}
          tabIndex={isDisabled ? -1 : tabIndex}
          onClick={onClick}
          {...otherProps}
        />
      );
    }

    return (
      <RouterLink
        to={to}
        target={target}
        className={linkClass}
        aria-disabled={isDisabled || undefined}
        tabIndex={isDisabled ? -1 : tabIndex}
        onClick={onClick}
        {...otherProps}
      />
    );
  }

  return (
    <Component
      type={
        component === 'button' || component === 'input'
          ? type || 'button'
          : type
      }
      target={target}
      className={linkClass}
      disabled={isDisabled}
      tabIndex={tabIndex}
      onClick={onClick}
      {...otherProps}
    />
  );
}
