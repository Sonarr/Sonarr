import React, { useCallback, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Episode from 'Episode/Episode';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import {
  clearEpisodeFiles,
  fetchEpisodeFiles,
} from 'Store/Actions/episodeFileActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import Agenda from './Agenda/Agenda';
import { useCalendarOption } from './calendarOptionsStore';
import CalendarDays from './Day/CalendarDays';
import DaysOfWeek from './Day/DaysOfWeek';
import CalendarHeader from './Header/CalendarHeader';
import useCalendar, { goToToday } from './useCalendar';
import styles from './Calendar.css';

const UPDATE_DELAY = 3600000; // 1 hour

function Calendar() {
  const dispatch = useDispatch();
  const requestCurrentPage = useCurrentPage();
  const updateTimeout = useRef<ReturnType<typeof setTimeout>>();

  const { data, isFetching, isLoading, error, refetch } = useCalendar();
  const view = useCalendarOption('view');

  const isRefreshingSeries = useSelector(
    createCommandExecutingSelector(commandNames.REFRESH_SERIES)
  );

  const wasRefreshingSeries = usePrevious(isRefreshingSeries);

  const handleScheduleUpdate = useCallback(() => {
    clearTimeout(updateTimeout.current);

    function updateCalendar() {
      goToToday();
      updateTimeout.current = setTimeout(updateCalendar, UPDATE_DELAY);
    }

    updateTimeout.current = setTimeout(updateCalendar, UPDATE_DELAY);
  }, []);

  useEffect(() => {
    handleScheduleUpdate();

    return () => {
      dispatch(clearEpisodeFiles());
      clearTimeout(updateTimeout.current);
    };
  }, [dispatch, handleScheduleUpdate]);

  useEffect(() => {
    if (!requestCurrentPage) {
      goToToday();
    }
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      refetch();
    };

    registerPagePopulator(repopulate, [
      'episodeFileUpdated',
      'episodeFileDeleted',
    ]);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [refetch]);

  useEffect(() => {
    handleScheduleUpdate();
  }, [handleScheduleUpdate]);

  useEffect(() => {
    if (wasRefreshingSeries && !isRefreshingSeries) {
      refetch();
    }
  }, [isRefreshingSeries, wasRefreshingSeries, refetch]);

  useEffect(() => {
    const episodeFileIds = selectUniqueIds<Episode, number>(
      data,
      'episodeFileId'
    );

    if (episodeFileIds.length) {
      dispatch(fetchEpisodeFiles({ episodeFileIds }));
    }
  }, [data, dispatch]);

  return (
    <div className={styles.calendar}>
      {isLoading ? <LoadingIndicator /> : null}
      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>{translate('CalendarLoadError')}</Alert>
      ) : null}
      {!error && !isLoading && view === 'agenda' ? (
        <div className={styles.calendarContent}>
          <CalendarHeader />
          <Agenda />
        </div>
      ) : null}
      {!error && !isLoading && view !== 'agenda' ? (
        <div className={styles.calendarContent}>
          <CalendarHeader />
          <DaysOfWeek />
          <CalendarDays />
        </div>
      ) : null}
    </div>
  );
}

export default Calendar;
