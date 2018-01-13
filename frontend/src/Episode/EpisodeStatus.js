import PropTypes from 'prop-types';
import React from 'react';
import isBefore from 'Utilities/Date/isBefore';
import { icons, kinds, sizes } from 'Helpers/Props';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import QueueDetails from 'Activity/Queue/QueueDetails';
import EpisodeQuality from './EpisodeQuality';
import styles from './EpisodeStatus.css';

function EpisodeStatus(props) {
  const {
    airDateUtc,
    monitored,
    grabbed,
    queueItem,
    episodeFile
  } = props;

  const hasEpisodeFile = !!episodeFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(airDateUtc);

  if (isQueued) {
    const {
      sizeleft,
      size
    } = queueItem;

    const progress = (100 - sizeleft / size * 100);

    return (
      <div className={styles.center}>
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              title={`Episode is downloading - ${progress.toFixed(1)}% ${queueItem.title}`}
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </div>
    );
  }

  if (grabbed) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.DOWNLOADING}
          title="Episode is downloading"
        />
      </div>
    );
  }

  if (hasEpisodeFile) {
    const quality = episodeFile.quality;
    const isCutoffNotMet = episodeFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <EpisodeQuality
          quality={quality}
          size={episodeFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title="Episode Downloaded"
        />
      </div>
    );
  }

  if (!airDateUtc) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.TBA}
          title="TBA"
        />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.UNMONITORED}
          title="Episode is not monitored"
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title="Episode missing from disk"
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon
        name={icons.NOT_AIRED}
        title="Episode has not aired"
      />
    </div>
  );
}

EpisodeStatus.propTypes = {
  airDateUtc: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  episodeFile: PropTypes.object
};

export default EpisodeStatus;
