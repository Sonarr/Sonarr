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
import styles from './InteractiveSearchCard.module.css';

interface InteractiveSearchCardProps extends Release {
  actions: React.ReactNode;
}

function InteractiveSearchCard(props: InteractiveSearchCardProps) {
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
    <div className={styles.card}>
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
        <div className={styles.group}>
          <EpisodeQuality
            className={styles.qualityLabel}
            quality={quality}
            showRevision={true}
          />
          <EpisodeLanguages languages={languages} />

          {customFormatScoreText ? (
            <Tooltip
              className={styles.score}
              anchor={customFormatScoreText}
              tooltip={<EpisodeFormats formats={customFormats} />}
              position={tooltipPositions.BOTTOM}
            />
          ) : null}

          <ReleaseStatusIcons
            decision={decision}
            history={history}
            release={release}
          />
        </div>

        <div className={styles.group}>
          <span className={styles.size}>{formatBytes(size)}</span>
          <span className={styles.separator}>·</span>

          <span
            className={styles.age}
            title={formatDateTime(publishDate, longDateFormat, timeFormat, {
              includeSeconds: true,
            })}
          >
            {formatAge(age, ageHours, ageMinutes)}
          </span>

          {protocol === 'torrent' ? (
            <Peers seeders={seeders} leechers={leechers} />
          ) : null}
        </div>

        <div className={styles.source}>
          <ProtocolLabel protocol={protocol} />

          <span className={styles.indexer} title={indexer}>
            {indexer}
          </span>

          <div className={styles.actions}>{actions}</div>
        </div>
      </div>
    </div>
  );
}

export default InteractiveSearchCard;
