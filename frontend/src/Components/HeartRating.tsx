import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './HeartRating.css';

interface HeartRatingProps {
  rating: number;
  votes?: number;
  iconSize?: IconProps['size'];
}

function HeartRating({ rating, votes = 0, iconSize = 14 }: HeartRatingProps) {
  return (
    <Tooltip
      anchor={
        <span className={styles.rating}>
          <Icon className={styles.heart} name={icons.HEART} size={iconSize} />
          {rating * 10}%
        </span>
      }
      tooltip={translate('CountVotes', { votes })}
      kind={kinds.INVERSE}
      position={tooltipPositions.TOP}
    />
  );
}

export default HeartRating;
