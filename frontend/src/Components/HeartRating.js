import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './HeartRating.css';

function HeartRating({ rating, votes, iconSize }) {
  return (
    <Tooltip
      anchor={
        <span className={styles.rating}>
          <Icon
            className={styles.heart}
            name={icons.HEART}
            size={iconSize}
          />

          {rating * 10}%
        </span>
      }
      tooltip={translate('CountVotes', { votes })}
      kind={kinds.INVERSE}
      position={tooltipPositions.TOP}
    />
  );
}

HeartRating.propTypes = {
  rating: PropTypes.number.isRequired,
  votes: PropTypes.number.isRequired,
  iconSize: PropTypes.number.isRequired
};

HeartRating.defaultProps = {
  votes: 0,
  iconSize: 14
};

export default HeartRating;
