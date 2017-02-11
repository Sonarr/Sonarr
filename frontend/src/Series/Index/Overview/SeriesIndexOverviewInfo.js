import PropTypes from 'prop-types';
import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import Icon from 'Components/Icon';
import styles from './SeriesIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.seriesIndexOverviewInfoRowHeight);

function isVisible(name, show, value, sortKey, index) {
  if (value == null) {
    return false;
  }

  return show || sortKey === name;
}

function SeriesIndexOverviewInfo(props) {
  const {
    height,
    showNetwork,
    showQualityProfile,
    showPreviousAiring,
    showAdded,
    showSeasonCount,
    showPath,
    showSizeOnDisk,
    nextAiring,
    network,
    qualityProfile,
    previousAiring,
    added,
    seasonCount,
    path,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  let seasons = '1 season';

  if (seasonCount === 0) {
    seasons = 'No seasons';
  } else if (seasonCount > 1) {
    seasons = `${seasonCount} seasons`;
  }

  const maxRows = Math.floor(height / (infoRowHeight + 4));

  return (
    <div className={styles.infos}>
      {
        !!nextAiring &&
          <div
            className={styles.info}
            title="Next Airing"
          >
            <Icon
              className={styles.icon}
              name={icons.SCHEDULED}
              size={14}
            />

            {
              getRelativeDate(
                nextAiring,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: true
                }
              )
            }
          </div>
      }

      {
        isVisible('network', showNetwork, network, sortKey) && maxRows > 1 &&
          <div
            className={styles.info}
            title="Network"
          >
            <Icon
              className={styles.icon}
              name={icons.NETWORK}
              size={14}
            />

            {network}
          </div>
      }

      {
        isVisible('qualityProfileId', showQualityProfile, qualityProfile, sortKey) && maxRows > 2 &&
          <div
            className={styles.info}
            title="Quality Profile"
          >
            <Icon
              className={styles.icon}
              name={icons.PROFILE}
              size={14}
            />

            {qualityProfile.name}
          </div>
      }

      {
        isVisible('previousAiring', showPreviousAiring, previousAiring, sortKey) && maxRows > 3 &&
          <div
            className={styles.info}
            title="Previous Airing"
          >
            <Icon
              className={styles.icon}
              name={icons.CALENDAR}
              size={14}
            />

            {
              getRelativeDate(
                previousAiring,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: true
                }
              )
            }
          </div>
      }

      {
        isVisible('added', showAdded, added, sortKey) && maxRows > 4 &&
          <div
            className={styles.info}
            title="Date Added"
          >
            <Icon
              className={styles.icon}
              name={icons.ADD}
              size={14}
            />

            {
              getRelativeDate(
                added,
                shortDateFormat,
                showRelativeDates,
                {
                  timeFormat,
                  timeForToday: true
                }
              )
            }
          </div>
      }

      {
        isVisible('seasonCount', showSeasonCount, seasonCount, sortKey) && maxRows > 5 &&
          <div
            className={styles.info}
            title="Season Count"
          >
            <Icon
              className={styles.icon}
              name={icons.CIRCLE}
              size={14}
            />

            {seasons}
          </div>
      }

      {
        isVisible('path', showPath, path, sortKey) && maxRows > 6 &&
          <div
            className={styles.info}
            title="Path"
          >
            <Icon
              className={styles.icon}
              name={icons.FOLDER}
              size={14}
            />

            {path}
          </div>
      }

      {
        isVisible('sizeOnDisk', showSizeOnDisk, sizeOnDisk, sortKey) && maxRows > 7 &&
          <div
            className={styles.info}
            title="Size on Disk"
          >
            <Icon
              className={styles.icon}
              name={icons.DRIVE}
              size={14}
            />

            {formatBytes(sizeOnDisk)}
          </div>
      }

    </div>
  );
}

SeriesIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showNetwork: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showPreviousAiring: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showSeasonCount: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  nextAiring: PropTypes.string,
  network: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  previousAiring: PropTypes.string,
  added: PropTypes.string,
  seasonCount: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  sortKey: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default SeriesIndexOverviewInfo;
