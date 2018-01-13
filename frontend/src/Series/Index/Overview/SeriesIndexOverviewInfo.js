import PropTypes from 'prop-types';
import React from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import SeriesIndexOverviewInfoRow from './SeriesIndexOverviewInfoRow';
import styles from './SeriesIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.seriesIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored'

  },
  {
    name: 'network',
    showProp: 'showNetwork',
    valueProp: 'network'
  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfileId'
  },
  {
    name: 'previousAiring',
    showProp: 'showPreviousAiring',
    valueProp: 'previousAiring'
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added'
  },
  {
    name: 'seasonCount',
    showProp: 'showSeasonCount',
    valueProp: 'seasonCount'
  },
  {
    name: 'path',
    showProp: 'showPath',
    valueProp: 'path'
  },
  {
    name: 'sizeOnDisk',
    showProp: 'showSizeOnDisk',
    valueProp: 'sizeOnDisk'
  }
];

function isVisible(row, props) {
  const {
    name,
    showProp,
    valueProp
  } = row;

  if (props[valueProp] == null) {
    return false;
  }

  return props[showProp] || props.sortKey === name;
}

function getInfoRowProps(row, props) {
  const { name } = row;

  if (name === 'monitored') {
    const monitoredText = props.monitored ? 'Monitored' : 'Unmonitored';

    return {
      title: monitoredText,
      iconName: props.monitored ? icons.MONITORED : icons.UNMONITORED,
      label: monitoredText
    };
  }

  if (name === 'network') {
    return {
      title: 'Network',
      iconName: icons.NETWORK,
      label: props.network
    };
  }

  if (name === 'qualityProfileId') {
    return {
      title: 'Quality PROFILE',
      iconName: icons.PROFILE,
      label: props.qualityProfile.name
    };
  }

  if (name === 'previousAiring') {
    const {
      previousAiring,
      shortDateFormat,
      showRelativeDates,
      timeFormat
    } = props;

    return {
      title: 'Previous Airing',
      iconName: icons.CALENDAR,
      label: getRelativeDate(
        previousAiring,
        shortDateFormat,
        showRelativeDates,
        {
          timeFormat,
          timeForToday: true
        }
      )
    };
  }

  if (name === 'added') {
    const {
      added,
      shortDateFormat,
      showRelativeDates,
      timeFormat
    } = props;

    return {
      title: 'Added',
      iconName: icons.ADD,
      label: getRelativeDate(
        added,
        shortDateFormat,
        showRelativeDates,
        {
          timeFormat,
          timeForToday: true
        }
      )
    };
  }

  if (name === 'seasonCount') {
    const { seasonCount } = props;
    let seasons = '1 season';

    if (seasonCount === 0) {
      seasons = 'No seasons';
    } else if (seasonCount > 1) {
      seasons = `${seasonCount} seasons`;
    }

    return {
      title: 'Season Count',
      iconName: icons.CIRCLE,
      label: seasons
    };
  }

  if (name === 'path') {
    return {
      title: 'Path',
      iconName: icons.FOLDER,
      label: props.path
    };
  }

  if (name === 'sizeOnDisk') {
    return {
      title: 'Size on Disk',
      iconName: icons.DRIVE,
      label: formatBytes(props.sizeOnDisk)
    };
  }
}

function SeriesIndexOverviewInfo(props) {
  const {
    height,
    nextAiring,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  return (
    <div className={styles.infos}>
      {
        !!nextAiring &&
        <SeriesIndexOverviewInfoRow
          title={nextAiring}
          iconName={icons.SCHEDULED}
          label={getRelativeDate(
            nextAiring,
            shortDateFormat,
            showRelativeDates,
            {
              timeFormat,
              timeForToday: true
            }
          )}
        />
      }

      {
        rows.map((row) => {
          if (!isVisible(row, props)) {
            return null;
          }

          if (shownRows >= maxRows) {
            return null;
          }

          shownRows++;

          const infoRowProps = getInfoRowProps(row, props);

          return (
            <SeriesIndexOverviewInfoRow
              key={row.name}
              {...infoRowProps}
            />
          );
        })
      }
    </div>
  );
}

SeriesIndexOverviewInfo.propTypes = {
  height: PropTypes.number.isRequired,
  showNetwork: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  showPreviousAiring: PropTypes.bool.isRequired,
  showAdded: PropTypes.bool.isRequired,
  showSeasonCount: PropTypes.bool.isRequired,
  showPath: PropTypes.bool.isRequired,
  showSizeOnDisk: PropTypes.bool.isRequired,
  monitored: PropTypes.bool.isRequired,
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
