import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import { kinds } from 'Helpers/Props';
import styles from './Icon.css';

class Icon extends PureComponent {

  //
  // Render

  render() {
    const {
      containerClassName,
      className,
      name,
      kind,
      size,
      title,
      darken,
      isSpinning,
      ...otherProps
    } = this.props;

    const icon = (
      <FontAwesomeIcon
        className={classNames(
          className,
          styles[kind],
          darken && 'darken'
        )}
        icon={name}
        spin={isSpinning}
        style={{
          fontSize: `${size}px`
        }}
        {...otherProps}
      />
    );

    if (title) {
      return (
        <span
          className={containerClassName}
          title={title}
        >
          {icon}
        </span>
      );
    }

    return icon;
  }
}

Icon.propTypes = {
  containerClassName: PropTypes.string,
  className: PropTypes.string,
  name: PropTypes.object.isRequired,
  kind: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  title: PropTypes.string,
  darken: PropTypes.bool.isRequired,
  isSpinning: PropTypes.bool.isRequired,
  fixedWidth: PropTypes.bool.isRequired
};

Icon.defaultProps = {
  kind: kinds.DEFAULT,
  size: 14,
  darken: false,
  isSpinning: false,
  fixedWidth: false
};

export default Icon;
