import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './MenuItem.css';

class MenuItem extends Component {

  //
  // Render

  render() {
    const {
      className,
      children,
      isDisabled,
      ...otherProps
    } = this.props;

    return (
      <Link
        className={classNames(
          className,
          isDisabled && styles.isDisabled
        )}
        isDisabled={isDisabled}
        {...otherProps}
      >
        {children}
      </Link>
    );
  }
}

MenuItem.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  isDisabled: PropTypes.bool.isRequired
};

MenuItem.defaultProps = {
  className: styles.menuItem,
  isDisabled: false
};

export default MenuItem;
