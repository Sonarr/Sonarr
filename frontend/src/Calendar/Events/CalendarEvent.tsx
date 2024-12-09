import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import episodeEntities from 'Episode/episodeEntities';
import getFinaleTypeName from 'Episode/getFinaleTypeName';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
import { icons, kinds } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

interface CalendarEventProps {
  id: number;
  episodeId: number;
  seriesId: number;
  episodeFileId?: number;
  title: string;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  airDateUtc: string;
  monitored: boolean;
  unverifiedSceneNumbering?: boolean;
  finaleType?: string;
  hasFile: boolean;
  grabbed?: boolean;
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEvent(props: CalendarEventProps) {
  const {
    id,
    seriesId,
    episodeFileId,
    title,
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    airDateUtc,
    monitored,
    unverifiedSceneNumbering,
    finaleType,
    hasFile,
    grabbed,
    onEventModalOpenToggle,
  } = props;

  const series = useSeries(seriesId);
  const episodeFile = useEpisodeFile(episodeFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));

  const { timeFormat, enableColorImpairedMode } = useSelector(
    createUISettingsSelector()
  );

  const {
    showEpisodeInformation,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(true);
    onEventModalOpenToggle(true);
  }, [onEventModalOpenToggle]);

  const handlePress = useCallback(() => {
    setIsDetailsModalOpen(false);
    onEventModalOpenToggle(false);
  }, [onEventModalOpenToggle]);

  if (!series) {
    return null;
  }

  const startTime = moment(airDateUtc);
  const endTime = moment(airDateUtc).add(series.runtime, 'minutes');
  const isDownloading = !!(queueItem || grabbed);
  const isMonitored = series.monitored && monitored;
  const statusStyle = getStatusStyle(
    hasFile,
    isDownloading,
    startTime,
    endTime,
    isMonitored
  );
  const missingAbsoluteNumber =
    series.seriesType === 'anime' && seasonNumber > 0 && !absoluteEpisodeNumber;

  return (
    <div
      className={classNames(
        styles.event,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <Link className={styles.underlay} onPress={handlePress} />

      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.seriesTitle}>{series.title}</div>

          <div
            className={classNames(
              styles.statusContainer,
              fullColorEvents && 'fullColor'
            )}
          >
            {missingAbsoluteNumber ? (
              <Icon
                className={styles.statusIcon}
                name={icons.WARNING}
                title={translate('EpisodeMissingAbsoluteNumber')}
              />
            ) : null}

            {unverifiedSceneNumbering && !missingAbsoluteNumber ? (
              <Icon
                className={styles.statusIcon}
                name={icons.WARNING}
                title={translate('SceneNumberNotVerified')}
              />
            ) : null}

            {queueItem ? (
              <span className={styles.statusIcon}>
                <CalendarEventQueueDetails {...queueItem} />
              </span>
            ) : null}

            {!queueItem && grabbed ? (
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('EpisodeIsDownloading')}
              />
            ) : null}

            {showCutoffUnmetIcon &&
            !!episodeFile &&
            episodeFile.qualityCutoffNotMet ? (
              <Icon
                className={styles.statusIcon}
                name={icons.EPISODE_FILE}
                kind={kinds.WARNING}
                title={translate('QualityCutoffNotMet')}
              />
            ) : null}

            {episodeNumber === 1 && seasonNumber > 0 ? (
              <Icon
                className={styles.statusIcon}
                name={icons.PREMIERE}
                kind={kinds.INFO}
                title={
                  seasonNumber === 1
                    ? translate('SeriesPremiere')
                    : translate('SeasonPremiere')
                }
              />
            ) : null}

            {showFinaleIcon && finaleType ? (
              <Icon
                className={styles.statusIcon}
                name={
                  finaleType === 'series'
                    ? icons.FINALE_SERIES
                    : icons.FINALE_SEASON
                }
                kind={finaleType === 'series' ? kinds.DANGER : kinds.WARNING}
                title={getFinaleTypeName(finaleType)}
              />
            ) : null}

            {showSpecialIcon && (episodeNumber === 0 || seasonNumber === 0) ? (
              <Icon
                className={styles.statusIcon}
                name={icons.INFO}
                kind={kinds.PINK}
                title={translate('Special')}
              />
            ) : null}
          </div>
        </div>

        {showEpisodeInformation ? (
          <div className={styles.episodeInfo}>
            <div className={styles.episodeTitle}>{title}</div>

            <div>
              {seasonNumber}x{padNumber(episodeNumber, 2)}
              {series.seriesType === 'anime' && absoluteEpisodeNumber ? (
                <span className={styles.absoluteEpisodeNumber}>
                  ({absoluteEpisodeNumber})
                </span>
              ) : null}
            </div>
          </div>
        ) : null}

        <div className={styles.airTime}>
          {formatTime(airDateUtc, timeFormat)} -{' '}
          {formatTime(endTime.toISOString(), timeFormat, {
            includeMinuteZero: true,
          })}
        </div>
      </div>

      <EpisodeDetailsModal
        isOpen={isDetailsModalOpen}
        episodeId={id}
        episodeEntity={episodeEntities.CALENDAR}
        seriesId={series.id}
        episodeTitle={title}
        showOpenSeriesButton={true}
        onModalClose={handleDetailsModalClose}
      />
    </div>
  );
}

export default CalendarEvent;
