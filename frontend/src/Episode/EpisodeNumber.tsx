import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import { AlternateTitle, SeriesType } from 'Series/Series';
import padNumber from 'Utilities/Number/padNumber';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import translate from 'Utilities/String/translate';
import SceneInfo from './SceneInfo';
import styles from './EpisodeNumber.css';

function getWarningMessage(
  unverifiedSceneNumbering: boolean,
  seriesType: SeriesType | undefined,
  absoluteEpisodeNumber: number | undefined
) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  if (seriesType === 'anime' && !absoluteEpisodeNumber) {
    messages.push(translate('EpisodeMissingAbsoluteNumber'));
  }

  return messages.join('\n');
}

export interface EpisodeNumberProps {
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  useSceneNumbering?: boolean;
  unverifiedSceneNumbering?: boolean;
  alternateTitles?: AlternateTitle[];
  seriesType?: SeriesType;
  showSeasonNumber?: boolean;
}

function EpisodeNumber(props: EpisodeNumberProps) {
  const {
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    useSceneNumbering = false,
    unverifiedSceneNumbering = false,
    alternateTitles: seriesAlternateTitles = [],
    seriesType,
    showSeasonNumber = false,
  } = props;

  const alternateTitles = filterAlternateTitles(
    seriesAlternateTitles,
    null,
    useSceneNumbering,
    seasonNumber,
    sceneSeasonNumber
  );

  const hasSceneInformation =
    sceneSeasonNumber !== undefined ||
    sceneEpisodeNumber !== undefined ||
    (seriesType === 'anime' && sceneAbsoluteEpisodeNumber !== undefined) ||
    !!alternateTitles.length;

  const warningMessage = getWarningMessage(
    unverifiedSceneNumbering,
    seriesType,
    absoluteEpisodeNumber
  );

  return (
    <span>
      {hasSceneInformation ? (
        <Popover
          anchor={
            <span>
              {showSeasonNumber && seasonNumber != null && <>{seasonNumber}x</>}

              {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

              {seriesType === 'anime' && !!absoluteEpisodeNumber && (
                <span className={styles.absoluteEpisodeNumber}>
                  ({absoluteEpisodeNumber})
                </span>
              )}
            </span>
          }
          title={translate('SceneInformation')}
          body={
            <SceneInfo
              seasonNumber={seasonNumber}
              episodeNumber={episodeNumber}
              sceneSeasonNumber={sceneSeasonNumber}
              sceneEpisodeNumber={sceneEpisodeNumber}
              sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
              alternateTitles={alternateTitles}
              seriesType={seriesType}
            />
          }
          position={tooltipPositions.RIGHT}
        />
      ) : (
        <span>
          {showSeasonNumber && seasonNumber != null && <>{seasonNumber}x</>}

          {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

          {seriesType === 'anime' && !!absoluteEpisodeNumber && (
            <span className={styles.absoluteEpisodeNumber}>
              ({absoluteEpisodeNumber})
            </span>
          )}
        </span>
      )}

      {warningMessage ? (
        <Icon
          className={styles.warning}
          name={icons.WARNING}
          kind={kinds.WARNING}
          title={warningMessage}
        />
      ) : null}
    </span>
  );
}

export default EpisodeNumber;
