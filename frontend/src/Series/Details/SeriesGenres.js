import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Tooltip from 'Components/Tooltip/Tooltip';
import { kinds, sizes, tooltipPositions } from 'Helpers/Props';
import styles from './SeriesGenres.css';

function SeriesGenres({ genres }) {
  const [firstGenre, ...otherGenres] = genres;

  if (otherGenres.length) {
    return (
      <Tooltip
        anchor={
          <span className={styles.genres}>
            {firstGenre}
          </span>
        }
        tooltip={
          <div>
            {
              otherGenres.map((tag) => {
                return (
                  <Label
                    key={tag}
                    kind={kinds.INFO}
                    size={sizes.LARGE}
                  >
                    {tag}
                  </Label>
                );
              })
            }
          </div>
        }
        kind={kinds.INVERSE}
        position={tooltipPositions.TOP}
      />
    );
  }

  return (
    <span className={styles.genres}>
      {firstGenre}
    </span>
  );
}

SeriesGenres.propTypes = {
  genres: PropTypes.arrayOf(PropTypes.string).isRequired
};

export default SeriesGenres;
