import PropTypes from 'prop-types';
import React from 'react';
import FontAwesomeIcon from '@fortawesome/react-fontawesome';
import { kinds } from 'Helpers/Props';
import classNames from 'classnames';
import styles from './Icon.css';

function Icon(props) {
  const {
    className,
    name,
    kind,
    size,
    title,
    isSpinning,
    ...otherProps
  } = props;

  return (
    <FontAwesomeIcon
      className={classNames(
        className,
        styles[kind]
      )}
      icon={name}
      spin={isSpinning}
      title={title}
      style={{
        fontSize: `${size}px`
      }}
      {...otherProps}
    />
  );
}

Icon.propTypes = {
  className: PropTypes.string,
  name: PropTypes.object.isRequired,
  kind: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  title: PropTypes.string,
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
