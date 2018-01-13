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

    if (to) {
      if (/\w+?:\/\//.test(to)) {
        el = 'a';
        linkProps.href = to;
        linkProps.target = target || '_blank';
      } else if (noRouter) {
        el = 'a';
        linkProps.href = to;
        linkProps.target = target || '_self';
      } else if (to.startsWith(window.Sonarr.urlBase)) {
        el = RouterLink;
        linkProps.to = to;
        linkProps.target = target;
      } else {
        el = RouterLink;
        linkProps.to = `${window.Sonarr.urlBase}/${to.replace(/^\//, '')}`;
        linkProps.target = target;
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
  to: PropTypes.string,
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
