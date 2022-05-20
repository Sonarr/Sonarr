import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { align, kinds, sizes } from 'Helpers/Props';
import Link from './Link';
import styles from './Button.css';

class Button extends Component {

  //
  // Render

  render() {
    const {
      className,
      buttonGroupPosition,
      kind,
      size,
      children,
      ...otherProps
    } = this.props;

    return (
      <Link
        className={classNames(
          className,
          styles[kind],
          styles[size],
          buttonGroupPosition && styles[buttonGroupPosition]
        )}
        {...otherProps}
      >
        {children}
      </Link>
    );
  }

}

Button.propTypes = {
  className: PropTypes.string.isRequired,
  buttonGroupPosition: PropTypes.oneOf(align.all),
  kind: PropTypes.oneOf(kinds.all),
  size: PropTypes.oneOf(sizes.all),
  children: PropTypes.node
};

Button.defaultProps = {
  className: styles.button,
  kind: kinds.DEFAULT,
  size: sizes.MEDIUM
};

export default Button;
