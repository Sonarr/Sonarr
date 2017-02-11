import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import styles from './MenuButton.css';

class MenuButton extends Component {

  //
  // Render

  render() {
    const {
      className,
      children,
      onPress,
      ...otherProps
    } = this.props;

    return (
      <Link
        className={className}
        onPress={onPress}
        {...otherProps}
      >
        {children}
      </Link>
    );
  }
}

MenuButton.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  onPress: PropTypes.func
};

MenuButton.defaultProps = {
  className: styles.menuButton
};

export default MenuButton;
