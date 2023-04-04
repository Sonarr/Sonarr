import classNames from 'classnames';
import React, {
  ComponentClass,
  FunctionComponent,
  SyntheticEvent,
  useCallback,
} from 'react';
import { Link as RouterLink } from 'react-router-dom';
import styles from './Link.css';

interface ReactRouterLinkProps {
  to?: string;
}

export interface LinkProps extends React.HTMLProps<HTMLAnchorElement> {
  className?: string;
  component?:
    | string
    | FunctionComponent<LinkProps>
    | ComponentClass<LinkProps, unknown>;
  to?: string;
  target?: string;
  isDisabled?: boolean;
  noRouter?: boolean;
  onPress?(event: SyntheticEvent): void;
}
function Link(props: LinkProps) {
  const {
    className,
    component = 'button',
    to,
    target,
    type,
    isDisabled,
    noRouter = false,
    onPress,
    ...otherProps
  } = props;

  const onClick = useCallback(
    (event: SyntheticEvent) => {
      if (!isDisabled && onPress) {
        onPress(event);
      }
    },
    [isDisabled, onPress]
  );

  const linkProps: React.HTMLProps<HTMLAnchorElement> & ReactRouterLinkProps = {
    target,
  };
  let el = component;

  if (to) {
    if (/\w+?:\/\//.test(to)) {
      el = 'a';
      linkProps.href = to;
      linkProps.target = target || '_blank';
      linkProps.rel = 'noreferrer';
    } else if (noRouter) {
      el = 'a';
      linkProps.href = to;
      linkProps.target = target || '_self';
    } else {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      el = RouterLink;
      linkProps.to = `${window.Sonarr.urlBase}/${to.replace(/^\//, '')}`;
      linkProps.target = target;
    }
  }

  if (el === 'button' || el === 'input') {
    linkProps.type = type || 'button';
    linkProps.disabled = isDisabled;
  }

  linkProps.className = classNames(
    className,
    styles.link,
    to && styles.to,
    isDisabled && 'isDisabled'
  );

  const elementProps = {
    ...otherProps,
    type,
    ...linkProps,
  };

  elementProps.onClick = onClick;

  return React.createElement(el, elementProps);
}

export default Link;
