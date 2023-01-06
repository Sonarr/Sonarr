import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './SeriesIndexPosterInfo.css';

interface SeriesIndexPosterInfoProps {
  network?: string;
  showQualityProfile: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  qualityProfile: any;
  previousAiring?: string;
  added?: string;
  seasonCount: number;
  path: string;
  sizeOnDisk?: number;
  sortKey: string;
  showRelativeDates: boolean;
  shortDateFormat: string;
  timeFormat: string;
}

function SeriesIndexPosterInfo(props: SeriesIndexPosterInfoProps) {
  const {
    network,
    qualityProfile,
    showQualityProfile,
    previousAiring,
    added,
    seasonCount,
    path,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat,
  } = props;

  if (sortKey === 'network' && network) {
    return <div className={styles.info}>{network}</div>;
  }

  if (sortKey === 'qualityProfileId' && !showQualityProfile) {
    return <div className={styles.info}>{qualityProfile.name}</div>;
  }

  if (sortKey === 'previousAiring' && previousAiring) {
    return (
      <div className={styles.info}>
        {getRelativeDate(previousAiring, shortDateFormat, showRelativeDates, {
          timeFormat,
          timeForToday: true,
        })}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate(
      added,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return <div className={styles.info}>{`Added ${addedDate}`}</div>;
  }

  if (sortKey === 'seasonCount') {
    let seasons = '1 season';

    if (seasonCount === 0) {
      seasons = 'No seasons';
    } else if (seasonCount > 1) {
      seasons = `${seasonCount} seasons`;
    }

    return <div className={styles.info}>{seasons}</div>;
  }

  if (sortKey === 'path') {
    return <div className={styles.info}>{path}</div>;
  }

  if (sortKey === 'sizeOnDisk') {
    return <div className={styles.info}>{formatBytes(sizeOnDisk)}</div>;
  }

  return null;
}

export default SeriesIndexPosterInfo;
