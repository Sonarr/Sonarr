import PropTypes from 'prop-types';
import React from 'react';
import styles from './DescriptionListItemDescription.css';

function DescriptionListItemDescription(props) {
  const {
    className,
    children
  } = props;

  return (
    <dd className={className}>
      {children}
    </dd>
  );
}

DescriptionListItemDescription.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.node])
};

DescriptionListItemDescription.defaultProps = {
  className: styles.description
};

export default DescriptionListItemDescription;
