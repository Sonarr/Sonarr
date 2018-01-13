import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { kinds } from 'Helpers/Props';
import styles from './Alert.css';

function Alert({ className, kind, children, ...otherProps }) {
  return (
    <div
      className={classNames(
        className,
        styles[kind]
      )}
      {...otherProps}
    >
      {children}
    </div>
  );
}

Alert.propTypes = {
  className: PropTypes.string.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  children: PropTypes.node.isRequired
};

Alert.defaultProps = {
  className: styles.alert,
  kind: kinds.INFO
};

export default Alert;
