import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import Link from 'Components/Link/Link';
import styles from './MenuButton.css';

class MenuButton extends Component {

  //
  // Render

  render() {
    const {
      className,
      children,
      isDisabled,
      onPress,
      ...otherProps
    } = this.props;

    return (
      <Link
        className={classNames(
          className,
          isDisabled && styles.isDisabled
        )}
        isDisabled={isDisabled}
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
  isDisabled: PropTypes.bool.isRequired,
  onPress: PropTypes.func
};

MenuButton.defaultProps = {
  className: styles.menuButton,
  isDisabled: false
};

export default MenuButton;
