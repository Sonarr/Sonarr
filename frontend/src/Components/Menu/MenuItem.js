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
      ...otherProps
    } = this.props;

    return (
      <Link
        className={className}
        {...otherProps}
      >
        {children}
      </Link>
    );
  }
}

MenuItem.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired
};

MenuItem.defaultProps = {
  className: styles.menuItem
};

export default MenuItem;
