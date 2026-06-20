import classNames from 'classnames';
import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './HeartRating.css';

interface HeartRatingProps {
  className?: string;
  rating: number;
  votes?: number;
  iconSize?: IconProps['size'];
}

function HeartRating({
  className,
  rating,
  votes = 0,
  iconSize = 14,
}: HeartRatingProps) {
  return (
    <Tooltip
      className={classNames(styles.rating, className)}
      anchor={
        <>
          <Icon
            className={styles.heart}
            name={icons.HEART}
            size={iconSize}
            filled={true}
          />
          {rating * 10}%
        </>
      }
      tooltip={translate('CountVotes', { votes })}
      kind={kinds.DEFAULT}
      position={tooltipPositions.TOP}
    />
  );
}

export default HeartRating;
