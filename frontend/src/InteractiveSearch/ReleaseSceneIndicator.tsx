import classNames from 'classnames';
import _ from 'lodash';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ReleaseSceneIndicator.css';

function formatSeasonRange(seasonNumbers: number[]): string {
  const sorted = [...seasonNumbers].sort((a, b) => a - b);

  const isConsecutive = sorted.every(
    (n, i) => i === 0 || n === sorted[i - 1] + 1
  );

  if (isConsecutive) {
    return `S${sorted[0]}–S${sorted[sorted.length - 1]}`;
  }

  return sorted.map((n) => `S${n}`).join(', ');
}

function formatReleaseNumber(
  seasonNumber: number | undefined,
  seasonNumbers: number[] | undefined,
  episodeNumbers: number[] | undefined,
  absoluteEpisodeNumbers: number[] | undefined,
  isMultiSeason: boolean
) {
  if (isMultiSeason && seasonNumbers && seasonNumbers.length > 1) {
    return formatSeasonRange(seasonNumbers);
  }

  if (episodeNumbers && episodeNumbers.length) {
    if (episodeNumbers.length > 1) {
      return `${seasonNumber}x${episodeNumbers[0]}-${
        episodeNumbers[episodeNumbers.length - 1]
      }`;
    }

    return `${seasonNumber}x${episodeNumbers[0]}`;
  }

  if (absoluteEpisodeNumbers && absoluteEpisodeNumbers.length) {
    if (absoluteEpisodeNumbers.length > 1) {
      return `${absoluteEpisodeNumbers[0]}-${
        absoluteEpisodeNumbers[absoluteEpisodeNumbers.length - 1]
      }`;
    }

    return absoluteEpisodeNumbers[0];
  }

  if (seasonNumber !== undefined) {
    return translate('SeasonNumberToken', { seasonNumber });
  }

  return null;
}

interface ReleaseSceneIndicatorProps {
  className: string;
  seasonNumber?: number;
  seasonNumbers?: number[];
  episodeNumbers?: number[];
  absoluteEpisodeNumbers?: number[];
  sceneSeasonNumber?: number;
  sceneSeasonNumbers?: number[];
  sceneEpisodeNumbers?: number[];
  sceneAbsoluteEpisodeNumbers?: number[];
  sceneMapping?: {
    sceneOrigin?: string;
    title?: string;
    comment?: string;
  };
  episodeRequested: boolean;
  isMultiSeason: boolean;
  isDaily: boolean;
}

function ReleaseSceneIndicator(props: ReleaseSceneIndicatorProps) {
  const {
    className,
    seasonNumber,
    seasonNumbers,
    episodeNumbers,
    absoluteEpisodeNumbers,
    sceneSeasonNumber,
    sceneSeasonNumbers,
    sceneEpisodeNumbers,
    sceneAbsoluteEpisodeNumbers,
    sceneMapping = {},
    episodeRequested,
    isMultiSeason,
    isDaily,
  } = props;

  const { sceneOrigin, title, comment } = sceneMapping;

  if (isDaily) {
    return null;
  }

  let mappingDifferent =
    sceneSeasonNumber !== undefined && seasonNumber !== sceneSeasonNumber;

  if (sceneEpisodeNumbers !== undefined) {
    mappingDifferent =
      mappingDifferent || !_.isEqual(sceneEpisodeNumbers, episodeNumbers);
  } else if (sceneAbsoluteEpisodeNumbers !== undefined) {
    mappingDifferent =
      mappingDifferent ||
      !_.isEqual(sceneAbsoluteEpisodeNumbers, absoluteEpisodeNumbers);
  }

  if (!sceneMapping && !mappingDifferent) {
    return null;
  }

  const releaseNumber = formatReleaseNumber(
    sceneSeasonNumber,
    sceneSeasonNumbers,
    sceneEpisodeNumbers,
    sceneAbsoluteEpisodeNumbers,
    isMultiSeason
  );
  const mappedNumber = formatReleaseNumber(
    seasonNumber,
    seasonNumbers,
    episodeNumbers,
    absoluteEpisodeNumbers,
    isMultiSeason
  );
  const messages = [];

  const isMixed = sceneOrigin === 'mixed';
  const isUnknown = sceneOrigin === 'unknown' || sceneOrigin === 'unknown:tvdb';

  let level = styles.levelNone;

  if (isMixed) {
    level = styles.levelMixed;
    messages.push(
      <div key="source">
        {translate('ReleaseSceneIndicatorSourceMessage', {
          message: comment ?? 'Source',
        })}
      </div>
    );
  } else if (isUnknown) {
    level = styles.levelUnknown;
    messages.push(
      <div key="unknown">
        {translate('ReleaseSceneIndicatorUnknownMessage')}
      </div>
    );

    if (sceneOrigin === 'unknown') {
      messages.push(
        <div key="origin">
          {translate('ReleaseSceneIndicatorAssumingScene')}.
        </div>
      );
    } else if (sceneOrigin === 'unknown:tvdb') {
      messages.push(
        <div key="origin">{translate('ReleaseSceneIndicatorAssumingTvdb')}</div>
      );
    }
  } else if (mappingDifferent) {
    level = styles.levelMapped;
  } else if (sceneOrigin) {
    level = styles.levelNormal;
  }

  if (!episodeRequested) {
    if (!isMixed && !isUnknown) {
      level = styles.levelNotRequested;
    }

    if (mappedNumber) {
      messages.push(
        <div key="not-requested">
          {translate('ReleaseSceneIndicatorMappedNotRequested')}
        </div>
      );
    } else {
      messages.push(
        <div key="unknown-series">
          {translate('ReleaseSceneIndicatorUnknownSeries')}
        </div>
      );
    }
  }

  const table = (
    <DescriptionList className={styles.descriptionList}>
      {comment !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Mapping')}
          data={comment}
        />
      )}

      {title !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Title')}
          data={title}
        />
      )}

      {releaseNumber !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Release')}
          data={releaseNumber ?? 'unknown'}
        />
      )}

      {releaseNumber !== undefined && (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('TheTvdb')}
          data={mappedNumber ?? 'unknown'}
        />
      )}
    </DescriptionList>
  );

  return (
    <Popover
      anchor={
        <div className={classNames(level, styles.container, className)}>
          <Icon name={icons.SCENE_MAPPING} />
        </div>
      }
      title={translate('SceneInfo')}
      body={
        <div>
          {table}
          {(messages.length && (
            <div className={styles.messages}>{messages}</div>
          )) ||
            null}
        </div>
      }
      position={tooltipPositions.RIGHT}
    />
  );
}

export default ReleaseSceneIndicator;
