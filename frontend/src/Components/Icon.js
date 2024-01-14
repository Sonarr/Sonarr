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
      isSpinning,
      ...otherProps
    } = this.props;

    const icon = (
      <FontAwesomeIcon
        className={classNames(
          className,
          styles[kind]
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
          title={typeof title === 'function' ? title() : title}
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
  title: PropTypes.oneOfType([PropTypes.string, PropTypes.func]),
  isSpinning: PropTypes.bool.isRequired,
  fixedWidth: PropTypes.bool.isRequired
};

Icon.defaultProps = {
  kind: kinds.DEFAULT,
  size: 14,
  isSpinning: false,
  fixedWidth: false
};

export default Icon;
