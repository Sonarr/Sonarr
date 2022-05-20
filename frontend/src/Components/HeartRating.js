import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import styles from './HeartRating.css';

function HeartRating({ rating, iconSize }) {
  return (
    <span className={styles.rating}>
      <Icon
        className={styles.heart}
        name={icons.HEART}
        size={iconSize}
      />

      {rating * 10}%
    </span>
  );
}

HeartRating.propTypes = {
  rating: PropTypes.number.isRequired,
  iconSize: PropTypes.number.isRequired
};

HeartRating.defaultProps = {
  iconSize: 14
};

export default HeartRating;
