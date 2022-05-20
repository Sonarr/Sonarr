import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import styles from './EnhancedSelectInputSelectedValue.css';

function EnhancedSelectInputSelectedValue(props) {
  const {
    className,
    children,
    isDisabled
  } = props;

  return (
    <div className={classNames(
      className,
      isDisabled && styles.isDisabled
    )}
    >
      {children}
    </div>
  );
}

EnhancedSelectInputSelectedValue.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.node,
  isDisabled: PropTypes.bool.isRequired
};

EnhancedSelectInputSelectedValue.defaultProps = {
  className: styles.selectedValue,
  isDisabled: false
};

export default EnhancedSelectInputSelectedValue;
