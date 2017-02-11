import PropTypes from 'prop-types';
import React from 'react';
import styles from './EnhancedSelectInputSelectedValue.css';

function EnhancedSelectInputSelectedValue(props) {
  const {
    className,
    children
  } = props;

  return (
    <div className={className}>
      {children}
    </div>
  );
}

EnhancedSelectInputSelectedValue.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.node
};

EnhancedSelectInputSelectedValue.defaultProps = {
  className: styles.selectedValue
};

export default EnhancedSelectInputSelectedValue;
