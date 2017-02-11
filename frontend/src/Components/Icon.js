import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import classNames from 'classnames';
import styles from './Icon.css';

function Icon(props) {
  const {
    className,
    name,
    kind,
    size,
    title
  } = props;

  return (
    <i
      className={classNames(
        name,
        className,
        styles[kind]
      )}
      title={title}
      style={{
        fontSize: `${size}px`
      }}
    />
  );
}

Icon.propTypes = {
  className: PropTypes.string,
  name: PropTypes.string.isRequired,
  kind: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  title: PropTypes.string
};

Icon.defaultProps = {
  kind: kinds.DEFAULT,
  size: 14
};

export default Icon;
