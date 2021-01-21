import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { tooltipPositions, icons, sizes } from 'Helpers/Props';
import styles from './ReleaseSceneIndicator.css';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Popover from 'Components/Tooltip/Popover';
import Icon from 'Components/Icon';

function formatReleaseNumber(seasonNumber, episodeNumbers, absoluteEpisodeNumbers) {
  if (episodeNumbers && episodeNumbers.length) {
    if (episodeNumbers.length > 1) {
      return `${seasonNumber}x${episodeNumbers[0]}-${episodeNumbers[episodeNumbers.length - 1]}`;
    }
    return `${seasonNumber}x${episodeNumbers[0]}`;
  }

  if (absoluteEpisodeNumbers && absoluteEpisodeNumbers.length) {
    if (absoluteEpisodeNumbers.length > 1) {
      return `${absoluteEpisodeNumbers[0]}-${absoluteEpisodeNumbers[absoluteEpisodeNumbers.length - 1]}`;
    }
    return absoluteEpisodeNumbers[0];
  }

  if (seasonNumber !== undefined) {
    return `Season ${seasonNumber}`;
  }

  return null;
}

function ReleaseSceneIndicator(props) {
  const {
    className,
    seasonNumber,
    episodeNumbers,
    absoluteEpisodeNumbers,
    sceneSeasonNumber,
    sceneEpisodeNumbers,
    sceneAbsoluteEpisodeNumbers,
    sceneMapping,
    episodeRequested,
    isDaily
  } = props;

  const {
    sceneOrigin,
    title,
    comment
  } = sceneMapping || {};

  if (isDaily) {
    return null;
  }

  let mappingDifferent = (sceneSeasonNumber !== undefined && seasonNumber !== sceneSeasonNumber);

  if (sceneEpisodeNumbers !== undefined) {
    mappingDifferent = mappingDifferent || !_.isEqual(sceneEpisodeNumbers, episodeNumbers);
  } else if (sceneAbsoluteEpisodeNumbers !== undefined) {
    mappingDifferent = mappingDifferent || !_.isEqual(sceneAbsoluteEpisodeNumbers, absoluteEpisodeNumbers);
  }

  if (!sceneMapping && !mappingDifferent) {
    return null;
  }

  const releaseNumber = formatReleaseNumber(sceneSeasonNumber, sceneEpisodeNumbers, sceneAbsoluteEpisodeNumbers);
  const mappedNumber = formatReleaseNumber(seasonNumber, episodeNumbers, absoluteEpisodeNumbers);
  const messages = [];

  const isMixed = (sceneOrigin === 'mixed');
  const isUnknown = (sceneOrigin === 'unknown' || sceneOrigin === 'unknown:tvdb');

  let level = styles.levelNone;

  if (isMixed) {
    level = styles.levelMixed;
    messages.push(<div>{comment ?? 'Source'} releases exist with ambiguous numbering, unable to reliably identify episode.</div>);
  } else if (isUnknown) {
    level = styles.levelUnknown;
    messages.push(<div>Numbering varies for this episode and release does not match any known mappings.</div>);
    if (sceneOrigin === 'unknown') {
      messages.push(<div>Assuming Scene numbering.</div>);
    } else if (sceneOrigin === 'unknown:tvdb') {
      messages.push(<div>Assuming TheTVDB numbering.</div>);
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
      messages.push(<div>Mapped episode wasn't requested in this search.</div>);
    } else {
      messages.push(<div>Unknown episode or series.</div>);
    }
  }

  const table = (
    <DescriptionList className={styles.descriptionList}>
      {
        comment !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Mapping"
            data={comment}
          />
      }

      {
        title !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Title"
            data={title}
          />
      }

      {
        releaseNumber !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Release"
            data={releaseNumber ?? 'unknown'}
          />
      }

      {
        releaseNumber !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="TheTVDB"
            data={mappedNumber ?? 'unknown'}
          />
      }
    </DescriptionList>
  );

  return (
    <Popover
      anchor={
        <div className={classNames(level, styles.container, className)}>
          <Icon
            name={icons.SCENE_MAPPING}
            size={sizes.SMALL}
          />
        </div>
      }
      title="Scene Info"
      body={
        <div>
          {table}
          {
            messages.length &&
              <div className={styles.messages}>
                {messages}
              </div> || null
          }
        </div>
      }
      position={tooltipPositions.RIGHT}
    />
  );
}

ReleaseSceneIndicator.propTypes = {
  className: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number,
  episodeNumbers: PropTypes.arrayOf(PropTypes.number),
  absoluteEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  sceneAbsoluteEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  sceneMapping: PropTypes.object.isRequired,
  episodeRequested: PropTypes.bool.isRequired,
  isDaily: PropTypes.bool.isRequired
};

export default ReleaseSceneIndicator;
