import PropTypes from 'prop-types';
import React from 'react';
import EpisodeNumber from './EpisodeNumber';

function SeasonEpisodeNumber(props) {
  const {
    airDate,
    seriesType,
    ...otherProps
  } = props;

  if (seriesType === 'daily' && airDate) {
    return (
      <span>{airDate}</span>
    );
  }

  return (
    <EpisodeNumber
      seriesType={seriesType}
      showSeasonNumber={true}
      {...otherProps}
    />
  );
}

SeasonEpisodeNumber.propTypes = {
  airDate: PropTypes.string,
  seriesType: PropTypes.string
};

export default SeasonEpisodeNumber;
