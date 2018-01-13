import PropTypes from 'prop-types';
import React from 'react';
import styles from './DescriptionListItemTitle.css';

function DescriptionListItemTitle(props) {
  const {
    className,
    children
  } = props;

  return (
    <dt className={className}>
      {children}
    </dt>
  );
}

DescriptionListItemTitle.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.string
};

DescriptionListItemTitle.defaultProps = {
  className: styles.title
};

export default DescriptionListItemTitle;
