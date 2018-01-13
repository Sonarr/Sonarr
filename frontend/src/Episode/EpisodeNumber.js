import PropTypes from 'prop-types';
import React from 'react';
import padNumber from 'Utilities/Number/padNumber';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import SceneInfo from './SceneInfo';
import styles from './EpisodeNumber.css';

function getAlternateTitles(seasonNumber, sceneSeasonNumber, alternateTitles) {
  return alternateTitles.filter((alternateTitle) => {
    if (sceneSeasonNumber && sceneSeasonNumber === alternateTitle.sceneSeasonNumber) {
      return true;
    }

    return seasonNumber === alternateTitle.seasonNumber;
  });
}

function EpisodeNumber(props) {
  const {
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    unverifiedSceneNumbering,
    alternateTitles: seriesAlternateTitles,
    seriesType,
    showSeasonNumber
  } = props;

  const alternateTitles = getAlternateTitles(seasonNumber, sceneSeasonNumber, seriesAlternateTitles);

  const hasSceneInformation = sceneSeasonNumber !== undefined ||
    sceneEpisodeNumber !== undefined ||
    (seriesType === 'anime' && sceneAbsoluteEpisodeNumber !== undefined) ||
    !!alternateTitles.length;

  return (
    <span>
      {
        hasSceneInformation ?
          <Popover
            anchor={
              <span>
                {showSeasonNumber && seasonNumber != null}
                {showSeasonNumber && 'x'}

                {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

                {
                  seriesType === 'anime' && !!absoluteEpisodeNumber &&
                    <span className={styles.absoluteEpisodeNumber}>
                      ({absoluteEpisodeNumber})
                    </span>
                }
              </span>
            }
            title="Scene Information"
            body={
              <SceneInfo
                sceneSeasonNumber={sceneSeasonNumber}
                sceneEpisodeNumber={sceneEpisodeNumber}
                sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
                alternateTitles={alternateTitles}
                seriesType={seriesType}
              />
            }
            position={tooltipPositions.RIGHT}
          /> :
          <span>
            {showSeasonNumber && seasonNumber}
            {showSeasonNumber && 'x'}

            {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

            {
              seriesType === 'anime' && !!absoluteEpisodeNumber &&
                <span className={styles.absoluteEpisodeNumber}>
                  ({absoluteEpisodeNumber})
                </span>
            }
          </span>
      }

      {
        unverifiedSceneNumbering &&
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title="Scene number hasn't been verified yet"
          />
      }

      {
        seriesType === 'anime' && !absoluteEpisodeNumber &&
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title="Episode does not have an absolute episode number"
          />
      }
    </span>
  );
}

EpisodeNumber.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  unverifiedSceneNumbering: PropTypes.bool.isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  seriesType: PropTypes.string,
  showSeasonNumber: PropTypes.bool.isRequired
};

EpisodeNumber.defaultProps = {
  unverifiedSceneNumbering: false,
  alternateTitles: [],
  showSeasonNumber: false
};

export default EpisodeNumber;
