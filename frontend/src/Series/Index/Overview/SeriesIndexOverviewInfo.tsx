import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import dimensions from 'Styles/Variables/dimensions';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import SeriesIndexOverviewInfoRow from './SeriesIndexOverviewInfoRow';
import styles from './SeriesIndexOverviewInfo.css';

const infoRowHeight = parseInt(dimensions.seriesIndexOverviewInfoRowHeight);

const rows = [
  {
    name: 'monitored',
    showProp: 'showMonitored',
    valueProp: 'monitored',
  },
  {
    name: 'network',
    showProp: 'showNetwork',
    valueProp: 'network',
  },
  {
    name: 'qualityProfileId',
    showProp: 'showQualityProfile',
    valueProp: 'qualityProfileId',
  },
  {
    name: 'previousAiring',
    showProp: 'showPreviousAiring',
    valueProp: 'previousAiring',
  },
  {
    name: 'added',
    showProp: 'showAdded',
    valueProp: 'added',
  },
  {
    name: 'seasonCount',
    showProp: 'showSeasonCount',
    valueProp: 'seasonCount',
  },
  {
    name: 'path',
    showProp: 'showPath',
    valueProp: 'path',
  },
  {
    name: 'sizeOnDisk',
    showProp: 'showSizeOnDisk',
    valueProp: 'sizeOnDisk',
  },
];

function getInfoRowProps(row, props, uiSettings) {
  const { name } = row;

  if (name === 'monitored') {
    const monitoredText = props.monitored ? 'Monitored' : 'Unmonitored';

    return {
      title: monitoredText,
      iconName: props.monitored ? icons.MONITORED : icons.UNMONITORED,
      label: monitoredText,
    };
  }

  if (name === 'network') {
    return {
      title: 'Network',
      iconName: icons.NETWORK,
      label: props.network,
    };
  }

  if (name === 'qualityProfileId') {
    return {
      title: 'Quality Profile',
      iconName: icons.PROFILE,
      label: props.qualityProfile.name,
    };
  }

  if (name === 'previousAiring') {
    const previousAiring = props.previousAiring;
    const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
      uiSettings;

    return {
      title: `Previous Airing: ${formatDateTime(
        previousAiring,
        longDateFormat,
        timeFormat
      )}`,
      iconName: icons.CALENDAR,
      label: getRelativeDate(
        previousAiring,
        shortDateFormat,
        showRelativeDates,
        {
          timeFormat,
          timeForToday: true,
        }
      ),
    };
  }

  if (name === 'added') {
    const added = props.added;
    const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
      uiSettings;

    return {
      title: `Added: ${formatDateTime(added, longDateFormat, timeFormat)}`,
      iconName: icons.ADD,
      label: getRelativeDate(added, shortDateFormat, showRelativeDates, {
        timeFormat,
        timeForToday: true,
      }),
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
      label: seasons,
    };
  }

  if (name === 'path') {
    return {
      title: 'Path',
      iconName: icons.FOLDER,
      label: props.path,
    };
  }

  if (name === 'sizeOnDisk') {
    return {
      title: 'Size on Disk',
      iconName: icons.DRIVE,
      label: formatBytes(props.sizeOnDisk),
    };
  }
}

interface SeriesIndexOverviewInfoProps {
  height: number;
  showNetwork: boolean;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showPreviousAiring: boolean;
  showAdded: boolean;
  showSeasonCount: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  monitored: boolean;
  nextAiring?: string;
  network?: string;
  qualityProfile: object;
  previousAiring?: string;
  added?: string;
  seasonCount: number;
  path: string;
  sizeOnDisk?: number;
  sortKey: string;
}

function SeriesIndexOverviewInfo(props: SeriesIndexOverviewInfoProps) {
  const { height, nextAiring } = props;

  const uiSettings = useSelector(createUISettingsSelector());

  const { shortDateFormat, showRelativeDates, longDateFormat, timeFormat } =
    uiSettings;

  let shownRows = 1;
  const maxRows = Math.floor(height / (infoRowHeight + 4));

  const rowInfo = useMemo(() => {
    return rows.map((row) => {
      const { name, showProp, valueProp } = row;

      const isVisible =
        props[valueProp] != null && (props[showProp] || props.sortKey === name);

      return {
        ...row,
        isVisible,
      };
    });
  }, [props]);

  return (
    <div className={styles.infos}>
      {!!nextAiring && (
        <SeriesIndexOverviewInfoRow
          title={formatDateTime(nextAiring, longDateFormat, timeFormat)}
          iconName={icons.SCHEDULED}
          label={getRelativeDate(
            nextAiring,
            shortDateFormat,
            showRelativeDates,
            {
              timeFormat,
              timeForToday: true,
            }
          )}
        />
      )}

      {rowInfo.map((row) => {
        if (!row.isVisible) {
          return null;
        }

        if (shownRows >= maxRows) {
          return null;
        }

        shownRows++;

        const infoRowProps = getInfoRowProps(row, props, uiSettings);

        return <SeriesIndexOverviewInfoRow key={row.name} {...infoRowProps} />;
      })}
    </div>
  );
}

export default SeriesIndexOverviewInfo;
