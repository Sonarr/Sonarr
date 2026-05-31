import classNames from 'classnames';
import React from 'react';
import { QualityProfileModel } from 'Settings/Profiles/Quality/useQualityProfiles';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { useTagList } from 'Tags/useTags';
import sortByProp from 'Utilities/Array/sortByProp';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './SeriesIndexOverviewInfo.css';

interface SeriesIndexOverviewInfoProps {
  showNetwork: boolean;
  showMonitored: boolean;
  showQualityProfile: boolean;
  showPreviousAiring: boolean;
  showAdded: boolean;
  showSeasonCount: boolean;
  showPath: boolean;
  showSizeOnDisk: boolean;
  showTags: boolean;
  monitored: boolean;
  nextAiring?: string;
  network?: string;
  qualityProfile?: QualityProfileModel;
  previousAiring?: string;
  added?: string;
  seasonCount: number;
  path: string;
  sizeOnDisk?: number;
  tags: number[];
  sortKey: string;
}

function SeriesIndexOverviewInfo(props: SeriesIndexOverviewInfoProps) {
  const {
    showNetwork,
    showMonitored,
    showQualityProfile,
    showPreviousAiring,
    showAdded,
    showSeasonCount,
    showPath,
    showSizeOnDisk,
    showTags,
    monitored,
    nextAiring,
    network,
    qualityProfile,
    previousAiring,
    added,
    seasonCount,
    path,
    sizeOnDisk = 0,
    tags,
    sortKey,
  } = props;

  const uiSettings = useUiSettingsValues();
  const { shortDateFormat, showRelativeDates, longDateFormat, timeFormat } =
    uiSettings;
  const tagList = useTagList();

  const chips: React.ReactNode[] = [];

  if (nextAiring) {
    chips.push(
      <span
        key="nextAiring"
        className={classNames(styles.chip, styles.chipAiring)}
        title={formatDateTime(nextAiring, longDateFormat, timeFormat)}
      >
        {getRelativeDate({
          date: nextAiring,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: true,
        })}
      </span>
    );
  }

  if (network && (showNetwork || sortKey === 'network')) {
    chips.push(
      <span key="network" className={styles.chip}>
        {network}
      </span>
    );
  }

  if (showMonitored || sortKey === 'monitored') {
    chips.push(
      <span key="monitored" className={styles.chip}>
        {monitored ? translate('Monitored') : translate('Unmonitored')}
      </span>
    );
  }

  if (
    qualityProfile?.name &&
    (showQualityProfile || sortKey === 'qualityProfileId')
  ) {
    chips.push(
      <span key="qualityProfile" className={styles.chip}>
        {qualityProfile.name}
      </span>
    );
  }

  if (previousAiring && (showPreviousAiring || sortKey === 'previousAiring')) {
    chips.push(
      <span
        key="previousAiring"
        className={styles.chip}
        title={formatDateTime(previousAiring, longDateFormat, timeFormat)}
      >
        <span className={styles.dateLabel}>{translate('Aired')}</span>
        {getRelativeDate({
          date: previousAiring,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: true,
        })}
      </span>
    );
  }

  if (added && (showAdded || sortKey === 'added')) {
    chips.push(
      <span
        key="added"
        className={styles.chip}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        <span className={styles.dateLabel}>{translate('Added')}</span>
        {getRelativeDate({
          date: added,
          shortDateFormat,
          showRelativeDates,
          timeFormat,
          timeForToday: true,
        })}
      </span>
    );
  }

  if (showSeasonCount || sortKey === 'seasonCount') {
    let seasons = translate('OneSeason');

    if (seasonCount === 0) {
      seasons = translate('NoSeasons');
    } else if (seasonCount > 1) {
      seasons = translate('CountSeasons', { count: seasonCount });
    }

    chips.push(
      <span key="seasonCount" className={styles.chip}>
        {seasons}
      </span>
    );
  }

  if (sizeOnDisk > 0 && (showSizeOnDisk || sortKey === 'sizeOnDisk')) {
    chips.push(
      <span key="sizeOnDisk" className={styles.chip}>
        {formatBytes(sizeOnDisk)}
      </span>
    );
  }

  if (showTags && tags.length > 0) {
    tags
      .map((id) => tagList.find((tag) => tag.id === id))
      .filter((tag): tag is NonNullable<typeof tag> => !!tag)
      .sort(sortByProp('label'))
      .forEach((tag) => {
        chips.push(
          <span
            key={`tag-${tag.id}`}
            className={classNames(styles.chip, styles.chipTag)}
          >
            {tag.label}
          </span>
        );
      });
  }

  if (path && (showPath || sortKey === 'path')) {
    chips.push(
      <span key="path" className={styles.chipPath}>
        {path}
      </span>
    );
  }

  if (chips.length === 0) {
    return null;
  }

  return <div className={styles.chipStrip}>{chips}</div>;
}

export default SeriesIndexOverviewInfo;
