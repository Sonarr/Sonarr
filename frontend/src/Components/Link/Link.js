import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import classNames from 'classnames';
import styles from './Link.css';

class Link extends Component {

  //
  // Listeners

  onClick = (event) => {
    const {
      isDisabled,
      onPress
    } = this.props;

    if (!isDisabled && onPress) {
      onPress(event);
    }
  }

  //
  // Render

  render() {
    const {
      className,
      component,
      to,
      target,
      isDisabled,
      noRouter,
      onPress,
      ...otherProps
    } = this.props;

    const linkProps = { target };
    let el = component;

    if (to && typeof to === 'string') {
      if ((/\w+?:\/\//).test(to)) {
        el = 'a';
        linkProps.href = to;
        linkProps.target = target || '_blank';
      } else if (noRouter) {
        el = 'a';
        linkProps.href = to;
        linkProps.target = target || '_self';
      } else {
        el = RouterLink;
        linkProps.to = `${window.Sonarr.urlBase}/${to.replace(/^\//, '')}`;
        linkProps.target = target;
      }
    } else if (to && typeof to === 'object') {
      el = RouterLink;
      linkProps.target = target;
      if (to.pathname.startsWith(`${window.Sonarr.urlBase}/`)) {
        linkProps.to = to;
      } else {
        const pathname = `${window.Sonarr.urlBase}/${to.pathname.replace(/^\//, '')}`;
        linkProps.to = {
          ...to,
          pathname
        };
      }
    }

    if (el === 'button' || el === 'input') {
      linkProps.type = otherProps.type || 'button';
      linkProps.disabled = isDisabled;
    }

    linkProps.className = classNames(
      className,
      styles.link,
      to && styles.to,
      isDisabled && 'isDisabled'
    );

    const props = {
      ...otherProps,
      ...linkProps
    };

    props.onClick = this.onClick;

    return (
      React.createElement(el, props)
    );
  }
}

Link.propTypes = {
  className: PropTypes.string,
  component: PropTypes.oneOfType([PropTypes.string, PropTypes.func]),
  to: PropTypes.oneOfType([PropTypes.string, PropTypes.object]),
  target: PropTypes.string,
  isDisabled: PropTypes.bool,
  noRouter: PropTypes.bool,
  onPress: PropTypes.func
};

Link.defaultProps = {
  component: 'button',
  noRouter: false
};

export default Link;
