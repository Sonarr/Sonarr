import PropTypes from 'prop-types';
import React from 'react';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import { sizes } from 'Helpers/Props';
import ProgressBar from 'Components/ProgressBar';
import styles from './SeriesIndexProgressBar.css';

function SeriesIndexProgressBar(props) {
  const {
    monitored,
    status,
    episodeCount,
    episodeFileCount,
    posterWidth,
    detailedProgressBar
  } = props;

  const progress = episodeCount ? episodeFileCount / episodeCount * 100 : 100;
  const text = `${episodeFileCount} / ${episodeCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, progress)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={detailedProgressBar ? null : text}
      width={posterWidth}
    />
  );
}

SeriesIndexProgressBar.propTypes = {
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  episodeCount: PropTypes.number.isRequired,
  episodeFileCount: PropTypes.number.isRequired,
  posterWidth: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired
};

export default SeriesIndexProgressBar;
