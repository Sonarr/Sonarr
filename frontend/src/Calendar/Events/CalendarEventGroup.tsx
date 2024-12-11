import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import getFinaleTypeName from 'Episode/getFinaleTypeName';
import { icons, kinds } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { CalendarItem } from 'typings/Calendar';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import CalendarEvent from './CalendarEvent';
import styles from './CalendarEventGroup.css';

function createIsDownloadingSelector(episodeIds: number[]) {
  return createSelector(
    (state: AppState) => state.queue.details,
    (details) => {
      return details.items.some((item) => {
        return !!(item.episodeId && episodeIds.includes(item.episodeId));
      });
    }
  );
}

interface CalendarEventGroupProps {
  episodeIds: number[];
  seriesId: number;
  events: CalendarItem[];
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEventGroup({
  episodeIds,
  seriesId,
  events,
  onEventModalOpenToggle,
}: CalendarEventGroupProps) {
  const isDownloading = useSelector(createIsDownloadingSelector(episodeIds));
  const series = useSeries(seriesId)!;

  const { timeFormat, enableColorImpairedMode } = useSelector(
    createUISettingsSelector()
  );

  const { showEpisodeInformation, showFinaleIcon, fullColorEvents } =
    useSelector((state: AppState) => state.calendar.options);

  const [isExpanded, setIsExpanded] = useState(false);

  const firstEpisode = events[0];
  const lastEpisode = events[events.length - 1];
  const airDateUtc = firstEpisode.airDateUtc;
  const startTime = moment(airDateUtc);
  const endTime = moment(lastEpisode.airDateUtc).add(series.runtime, 'minutes');
  const seasonNumber = firstEpisode.seasonNumber;

  const { allDownloaded, anyQueued, anyMonitored, allAbsoluteEpisodeNumbers } =
    useMemo(() => {
      let files = 0;
      let queued = 0;
      let monitored = 0;
      let absoluteEpisodeNumbers = 0;

      events.forEach((event) => {
        if (event.episodeFileId) {
          files++;
        }

        if (event.queued) {
          queued++;
        }

        if (series.monitored && event.monitored) {
          monitored++;
        }

        if (event.absoluteEpisodeNumber) {
          absoluteEpisodeNumbers++;
        }
      });

      return {
        allDownloaded: files === events.length,
        anyQueued: queued > 0,
        anyMonitored: monitored > 0,
        allAbsoluteEpisodeNumbers: absoluteEpisodeNumbers === events.length,
      };
    }, [series, events]);

  const anyDownloading = isDownloading || anyQueued;

  const statusStyle = getStatusStyle(
    allDownloaded,
    anyDownloading,
    startTime,
    endTime,
    anyMonitored
  );
  const isMissingAbsoluteNumber =
    series.seriesType === 'anime' &&
    seasonNumber > 0 &&
    !allAbsoluteEpisodeNumbers;

  const handleExpandPress = useCallback(() => {
    setIsExpanded((state) => !state);
  }, []);

  if (isExpanded) {
    return (
      <div>
        {events.map((event) => {
          return (
            <CalendarEvent
              key={event.id}
              episodeId={event.id}
              {...event}
              onEventModalOpenToggle={onEventModalOpenToggle}
            />
          );
        })}

        <Link
          className={styles.collapseContainer}
          component="div"
          onPress={handleExpandPress}
        >
          <Icon name={icons.COLLAPSE} />
        </Link>
      </div>
    );
  }

  return (
    <div
      className={classNames(
        styles.eventGroup,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <div className={styles.info}>
        <div className={styles.seriesTitle}>{series.title}</div>

        <div
          className={classNames(
            styles.statusContainer,
            fullColorEvents && 'fullColor'
          )}
        >
          {isMissingAbsoluteNumber ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.WARNING}
              title={translate('EpisodeMissingAbsoluteNumber')}
            />
          ) : null}

          {anyDownloading ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.DOWNLOADING}
              title={translate('AnEpisodeIsDownloading')}
            />
          ) : null}

          {firstEpisode.episodeNumber === 1 && seasonNumber > 0 ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={icons.PREMIERE}
              kind={kinds.INFO}
              title={
                seasonNumber === 1
                  ? translate('SeriesPremiere')
                  : translate('SeasonPremiere')
              }
            />
          ) : null}

          {showFinaleIcon && lastEpisode.finaleType ? (
            <Icon
              containerClassName={styles.statusIcon}
              name={
                lastEpisode.finaleType === 'series'
                  ? icons.FINALE_SERIES
                  : icons.FINALE_SEASON
              }
              kind={
                lastEpisode.finaleType === 'series'
                  ? kinds.DANGER
                  : kinds.WARNING
              }
              title={getFinaleTypeName(lastEpisode.finaleType)}
            />
          ) : null}
        </div>
      </div>

      <div className={styles.airingInfo}>
        <div className={styles.airTime}>
          {formatTime(airDateUtc, timeFormat)} -{' '}
          {formatTime(endTime.toISOString(), timeFormat, {
            includeMinuteZero: true,
          })}
        </div>

        {showEpisodeInformation ? (
          <div className={styles.episodeInfo}>
            {seasonNumber}x{padNumber(firstEpisode.episodeNumber, 2)}-
            {padNumber(lastEpisode.episodeNumber, 2)}
            {series.seriesType === 'anime' &&
            firstEpisode.absoluteEpisodeNumber &&
            lastEpisode.absoluteEpisodeNumber ? (
              <span className={styles.absoluteEpisodeNumber}>
                ({firstEpisode.absoluteEpisodeNumber}-
                {lastEpisode.absoluteEpisodeNumber})
              </span>
            ) : null}
          </div>
        ) : (
          <Link
            className={styles.expandContainerInline}
            component="div"
            onPress={handleExpandPress}
          >
            <Icon name={icons.EXPAND} />
          </Link>
        )}
      </div>

      {showEpisodeInformation ? (
        <Link
          className={styles.expandContainer}
          component="div"
          onPress={handleExpandPress}
        >
          &nbsp;
          <Icon name={icons.EXPAND} />
          &nbsp;
        </Link>
      ) : null}
    </div>
  );
}

export default CalendarEventGroup;
