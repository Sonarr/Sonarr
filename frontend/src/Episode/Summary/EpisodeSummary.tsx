import { useQueryClient } from '@tanstack/react-query';
import moment from 'moment-timezone';
import React, { useCallback, useContext, useEffect } from 'react';
import Episode from 'Episode/Episode';
import useEpisode, { EpisodeEntity } from 'Episode/useEpisode';
import {
  EpisodeFileContext,
  useEpisodeFile,
} from 'EpisodeFile/EpisodeFileProvider';
import { useDeleteEpisodeFile } from 'EpisodeFile/useEpisodeFiles';
import Series from 'Series/Series';
import { useSingleSeries } from 'Series/useSeries';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import formatDateTime from 'Utilities/Date/formatDateTime';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import EpisodeFileCard from './EpisodeFileCard';
import styles from './EpisodeSummary.module.css';

interface EpisodeSummaryProps {
  seriesId: number;
  episodeId: number;
  episodeEntity: EpisodeEntity;
  episodeFileId?: number;
}

function EpisodeSummary({
  seriesId,
  episodeId,
  episodeEntity,
  episodeFileId,
}: EpisodeSummaryProps) {
  const queryClient = useQueryClient();
  const { qualityProfileId, network } = useSingleSeries(seriesId) as Series;

  const { airDateUtc, overview } = useEpisode(
    episodeId,
    episodeEntity
  ) as Episode;

  const { shortDateFormat, showRelativeDates, timeFormat } =
    useUiSettingsValues();

  const hasEpisodeFileContext = !!useContext(EpisodeFileContext);
  const episodeFile = useEpisodeFile(episodeFileId);

  const { deleteEpisodeFile } = useDeleteEpisodeFile(
    episodeFileId!,
    episodeEntity
  );

  const handleDeleteEpisodeFile = useCallback(() => {
    deleteEpisodeFile();
  }, [deleteEpisodeFile]);

  const episodeFilePath = episodeFile?.path;

  useEffect(() => {
    if (hasEpisodeFileContext && episodeFileId && !episodeFilePath) {
      queryClient.invalidateQueries({ queryKey: ['/episodeFile'] });
    }
  }, [hasEpisodeFileContext, episodeFileId, episodeFilePath, queryClient]);

  return (
    <div>
      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>
            {isBefore(airDateUtc) ? translate('Aired') : translate('Airs')}
          </span>

          <span className={styles.metaValue}>
            {airDateUtc ? (
              <>
                {formatDateTime(airDateUtc, shortDateFormat, timeFormat, {
                  includeRelativeDay: showRelativeDates,
                })}

                {showRelativeDates ? (
                  <span className={styles.relative}>
                    {' '}
                    ({moment(airDateUtc).fromNow()})
                  </span>
                ) : null}
              </>
            ) : (
              translate('Tba')
            )}
          </span>
        </div>

        {network ? (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>{translate('Network')}</span>
            <span className={styles.metaValue}>{network}</span>
          </div>
        ) : null}

        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>
            {translate('QualityProfile')}
          </span>

          <span className={styles.metaValue}>
            <QualityProfileName qualityProfileId={qualityProfileId} />
          </span>
        </div>
      </div>

      <div className={styles.overview}>
        {overview || translate('NoEpisodeOverview')}
      </div>

      {episodeFile?.path ? (
        <EpisodeFileCard
          episodeFile={episodeFile}
          onDeleteEpisodeFile={handleDeleteEpisodeFile}
        />
      ) : null}
    </div>
  );
}

export default EpisodeSummary;
