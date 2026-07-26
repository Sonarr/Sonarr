import moment from 'moment';
import React, { useCallback, useMemo } from 'react';
import { useAppDimensions } from 'App/appStore';
import {
  setCalendarOption,
  useCalendarOption,
} from 'Calendar/calendarOptionsStore';
import { CalendarView } from 'Calendar/calendarViews';
import getCalendarViewLabel from 'Calendar/getCalendarViewLabel';
import useCalendar, {
  goToNextRange,
  goToPreviousRange,
  goToToday,
  useCalendarRange,
  useCalendarTime,
} from 'Calendar/useCalendar';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ScreenReaderOnly from 'Components/ScreenReaderOnly';
import { icons } from 'Helpers/Props';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import CalendarHeaderViewButton from './CalendarHeaderViewButton';
import styles from './CalendarHeader.css';

function CalendarHeader() {
  const { isFetching } = useCalendar();
  const view = useCalendarOption('view');
  const time = useCalendarTime();
  const { start, end } = useCalendarRange();

  const { isSmallScreen } = useAppDimensions();

  const { longDateFormat } = useUiSettingsValues();

  const handleViewChange = useCallback((newView: string) => {
    setCalendarOption('view', newView as CalendarView);
  }, []);

  const handleTodayPress = useCallback(() => {
    goToToday();
  }, []);

  const handlePreviousPress = useCallback(() => {
    goToPreviousRange();
  }, []);

  const handleNextPress = useCallback(() => {
    goToNextRange();
  }, []);

  const title = useMemo(() => {
    const timeMoment = moment(time);
    const startMoment = moment(start);
    const endMoment = moment(end);

    if (view === 'day') {
      return timeMoment.format(longDateFormat);
    } else if (view === 'month') {
      return timeMoment.format('MMMM YYYY');
    } else if (view === 'agenda') {
      return translate('Agenda');
    }

    let startFormat = 'MMM D YYYY';
    let endFormat = 'MMM D YYYY';

    if (startMoment.isSame(endMoment, 'month')) {
      startFormat = 'MMM D';
      endFormat = 'D YYYY';
    } else if (startMoment.isSame(endMoment, 'year')) {
      startFormat = 'MMM D';
      endFormat = 'MMM D YYYY';
    }

    return `${startMoment.format(startFormat)} \u2014 ${endMoment.format(
      endFormat
    )}`;
  }, [time, start, end, view, longDateFormat]);
  const viewLabel = getCalendarViewLabel(view);

  return (
    <div>
      {isSmallScreen ? (
        <h2
          className={styles.titleMobile}
          aria-live="polite"
          aria-atomic={true}
        >
          {title}
        </h2>
      ) : null}

      <div className={styles.header}>
        <div
          className={styles.navigationButtons}
          role="group"
          aria-label={translate('CalendarNavigation')}
        >
          <Button
            buttonGroupPosition="left"
            isDisabled={view === 'agenda'}
            aria-label={translate('CalendarPreviousRange', {
              view: viewLabel,
            })}
            onPress={handlePreviousPress}
          >
            <Icon name={icons.PAGE_PREVIOUS} aria-hidden={true} />
          </Button>

          <Button
            buttonGroupPosition="right"
            isDisabled={view === 'agenda'}
            aria-label={translate('CalendarNextRange', {
              view: viewLabel,
            })}
            onPress={handleNextPress}
          >
            <Icon name={icons.PAGE_NEXT} aria-hidden={true} />
          </Button>

          <Button
            className={styles.todayButton}
            isDisabled={view === 'agenda'}
            onPress={handleTodayPress}
          >
            {translate('Today')}
          </Button>
        </div>

        {isSmallScreen ? null : (
          <h2
            className={styles.titleDesktop}
            aria-live="polite"
            aria-atomic={true}
          >
            {title}
          </h2>
        )}

        <div
          className={styles.viewButtonsContainer}
          role="group"
          aria-label={translate('CalendarView')}
        >
          {isFetching ? (
            <>
              <LoadingIndicator className={styles.loading} size={20} />
              <ScreenReaderOnly role="status">
                {translate('Loading')}
              </ScreenReaderOnly>
            </>
          ) : null}

          <CalendarHeaderViewButton
            view="month"
            selectedView={view}
            buttonGroupPosition="left"
            onPress={handleViewChange}
          />

          <CalendarHeaderViewButton
            view="week"
            selectedView={view}
            buttonGroupPosition="center"
            onPress={handleViewChange}
          />

          <CalendarHeaderViewButton
            view="forecast"
            selectedView={view}
            buttonGroupPosition="center"
            onPress={handleViewChange}
          />

          <CalendarHeaderViewButton
            view="day"
            selectedView={view}
            buttonGroupPosition="center"
            onPress={handleViewChange}
          />

          <CalendarHeaderViewButton
            view="agenda"
            selectedView={view}
            buttonGroupPosition="right"
            onPress={handleViewChange}
          />
        </div>
      </div>
    </div>
  );
}

export default CalendarHeader;
