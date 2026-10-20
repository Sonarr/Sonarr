import React, { useMemo } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Link from 'Components/Link/Link';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { tooltipPositions } from 'Helpers/Props';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import Peers from './Peers';
import ReleaseSceneIndicator from './ReleaseSceneIndicator';
import ReleaseStatusIcons from './ReleaseStatusIcons';
import ReleaseTitle from './ReleaseTitle';
import { Release } from './useReleases';
import styles from './InteractiveSearchReleaseRow.module.css';

interface InteractiveSearchReleaseRowProps extends Release {
  actions: React.ReactNode;
}

function InteractiveSearchReleaseRow(props: InteractiveSearchReleaseRowProps) {
  const {
    decision,
    history,
    parsedInfo,
    release,
    languages,
    customFormatScore,
    customFormats,
    sceneMapping,
    mappedEpisodeNumbers,
    mappedAbsoluteEpisodeNumbers,
    mappedEpisodeInfo,
    episodeRequested,
    actions,
  } = props;

  const {
    absoluteEpisodeNumbers,
    episodeNumbers,
    isDaily,
    seasonNumbers,
    quality,
  } = parsedInfo;

  const {
    age,
    ageHours,
    ageMinutes,
    publishDate,
    title,
    infoUrl,
    indexer,
    size,
    seeders,
    leechers,
    protocol,
  } = release;

  const { longDateFormat, timeFormat } = useUiSettingsValues();

  const customFormatScoreText = formatCustomFormatScore(
    customFormatScore,
    customFormats.length
  );

  const mappedSeasonNumbers = useMemo(() => {
    return [...new Set(mappedEpisodeInfo.map((e) => e.seasonNumber))].sort(
      (a, b) => a - b
    );
  }, [mappedEpisodeInfo]);

  return (
    <div className={styles.row}>
      <div className={styles.release}>
        <div className={styles.title}>
          <Link to={infoUrl}>
            <ReleaseTitle title={title} />
          </Link>
          <ReleaseSceneIndicator
            className={styles.sceneMapping}
            seasonNumbers={mappedSeasonNumbers}
            episodeNumbers={mappedEpisodeNumbers}
            absoluteEpisodeNumbers={mappedAbsoluteEpisodeNumbers}
            sceneSeasonNumbers={seasonNumbers}
            sceneEpisodeNumbers={episodeNumbers}
            sceneAbsoluteEpisodeNumbers={absoluteEpisodeNumbers}
            sceneMapping={sceneMapping}
            episodeRequested={episodeRequested}
            isDaily={isDaily}
          />
        </div>

        <div className={styles.details}>
          <EpisodeLanguages languages={languages} />
          <ProtocolLabel protocol={protocol} />
          <span className={styles.indexer} title={indexer}>
            {indexer}
          </span>

          <ReleaseStatusIcons
            decision={decision}
            history={history}
            release={release}
          />
        </div>
      </div>

      <div
        className={styles.age}
        title={formatDateTime(publishDate, longDateFormat, timeFormat, {
          includeSeconds: true,
        })}
      >
        {formatAge(age, ageHours, ageMinutes)}
      </div>

      <div className={styles.size}>{formatBytes(size)}</div>

      <div className={styles.peers}>
        {protocol === 'torrent' ? (
          <Peers seeders={seeders} leechers={leechers} />
        ) : null}
      </div>

      <div className={styles.quality}>
        <EpisodeQuality
          className={styles.qualityLabel}
          quality={quality}
          showRevision={true}
        />
      </div>

      <div className={styles.score}>
        {customFormatScoreText ? (
          <Tooltip
            anchor={customFormatScoreText}
            tooltip={<EpisodeFormats formats={customFormats} />}
            position={tooltipPositions.BOTTOM}
          />
        ) : null}
      </div>

      <div className={styles.actions}>{actions}</div>
    </div>
  );
}

export default InteractiveSearchReleaseRow;
