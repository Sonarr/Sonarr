import React, { useCallback, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Episode from 'Episode/Episode';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import {
  clearCalendar,
  fetchCalendar,
  gotoCalendarToday,
} from 'Store/Actions/calendarActions';
import {
  clearEpisodeFiles,
  fetchEpisodeFiles,
} from 'Store/Actions/episodeFileActions';
import {
  clearQueueDetails,
  fetchQueueDetails,
} from 'Store/Actions/queueActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import Agenda from './Agenda/Agenda';
import CalendarDays from './Day/CalendarDays';
import DaysOfWeek from './Day/DaysOfWeek';
import CalendarHeader from './Header/CalendarHeader';
import styles from './Calendar.css';

const UPDATE_DELAY = 3600000; // 1 hour

function Calendar() {
  const dispatch = useDispatch();
  const requestCurrentPage = useCurrentPage();
  const updateTimeout = useRef<ReturnType<typeof setTimeout>>();

  const { isFetching, isPopulated, error, items, time, view } = useSelector(
    (state: AppState) => state.calendar
  );

  const isRefreshingSeries = useSelector(
    createCommandExecutingSelector(commandNames.REFRESH_SERIES)
  );

  const firstDayOfWeek = useSelector(
    (state: AppState) => state.settings.ui.item.firstDayOfWeek
  );

  const wasRefreshingSeries = usePrevious(isRefreshingSeries);
  const previousFirstDayOfWeek = usePrevious(firstDayOfWeek);
  const previousItems = usePrevious(items);

  const handleScheduleUpdate = useCallback(() => {
    clearTimeout(updateTimeout.current);

    function updateCalendar() {
      dispatch(gotoCalendarToday());
      updateTimeout.current = setTimeout(updateCalendar, UPDATE_DELAY);
    }

    updateTimeout.current = setTimeout(updateCalendar, UPDATE_DELAY);
  }, [dispatch]);

  useEffect(() => {
    handleScheduleUpdate();

    return () => {
      dispatch(clearCalendar());
      dispatch(clearQueueDetails());
      dispatch(clearEpisodeFiles());
      clearTimeout(updateTimeout.current);
    };
  }, [dispatch, handleScheduleUpdate]);

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchCalendar());
    } else {
      dispatch(gotoCalendarToday());
    }
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      dispatch(fetchQueueDetails({ time, view }));
      dispatch(fetchCalendar({ time, view }));
    };

    registerPagePopulator(repopulate, [
      'episodeFileUpdated',
      'episodeFileDeleted',
    ]);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [time, view, dispatch]);

  useEffect(() => {
    handleScheduleUpdate();
  }, [time, handleScheduleUpdate]);

  useEffect(() => {
    if (
      previousFirstDayOfWeek != null &&
      firstDayOfWeek !== previousFirstDayOfWeek
    ) {
      dispatch(fetchCalendar({ time, view }));
    }
  }, [time, view, firstDayOfWeek, previousFirstDayOfWeek, dispatch]);

  useEffect(() => {
    if (wasRefreshingSeries && !isRefreshingSeries) {
      dispatch(fetchCalendar({ time, view }));
    }
  }, [time, view, isRefreshingSeries, wasRefreshingSeries, dispatch]);

  useEffect(() => {
    if (!previousItems || hasDifferentItems(items, previousItems)) {
      const episodeIds = selectUniqueIds<Episode, number>(items, 'id');
      const episodeFileIds = selectUniqueIds<Episode, number>(
        items,
        'episodeFileId'
      );

      if (items.length) {
        dispatch(fetchQueueDetails({ episodeIds }));
      }

      if (episodeFileIds.length) {
        dispatch(fetchEpisodeFiles({ episodeFileIds }));
      }
    }
  }, [items, previousItems, dispatch]);

  return (
    <div className={styles.calendar}>
      {isFetching && !isPopulated ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>{translate('CalendarLoadError')}</Alert>
      ) : null}

      {!error && isPopulated && view === 'agenda' ? (
        <div className={styles.calendarContent}>
          <CalendarHeader />
          <Agenda />
        </div>
      ) : null}

      {!error && isPopulated && view !== 'agenda' ? (
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
