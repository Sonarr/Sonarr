import PropTypes from 'prop-types';
import React from 'react';
import padNumber from 'Utilities/Number/padNumber';
import styles from './SeasonEpisodeNumber.css';

function SeasonEpisodeNumber(props) {
  const {
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    airDate,
    seriesType
  } = props;

  if (seriesType === 'daily' && airDate) {
    return (
      <span>{airDate}</span>
    );
  }

  if (seriesType === 'anime') {
    return (
      <span>
        {seasonNumber}x{padNumber(episodeNumber, 2)}

        {
          absoluteEpisodeNumber &&
            <span className={styles.absoluteEpisodeNumber}>
              ({absoluteEpisodeNumber})
            </span>
        }
      </span>
    );
  }

  return (
    <span>
      {seasonNumber}x{padNumber(episodeNumber, 2)}
    </span>
  );
}

SeasonEpisodeNumber.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDate: PropTypes.string,
  seriesType: PropTypes.string
};

export default SeasonEpisodeNumber;
