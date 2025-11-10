import moment from 'moment';
import React, { useCallback, useMemo } from 'react';
import { useSelector } from 'react-redux';
import {
  setCalendarOption,
  useCalendarOption,
} from 'Calendar/calendarOptionsStore';
import { CalendarView } from 'Calendar/calendarViews';
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
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import ViewMenuItem from 'Components/Menu/ViewMenuItem';
import { align, icons } from 'Helpers/Props';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import CalendarHeaderViewButton from './CalendarHeaderViewButton';
import styles from './CalendarHeader.css';

function CalendarHeader() {
  const { isFetching } = useCalendar();
  const view = useCalendarOption('view');
  const time = useCalendarTime();
  const { start, end } = useCalendarRange();

  const { isSmallScreen, isLargeScreen } = useSelector(
    createDimensionsSelector()
  );

  const { longDateFormat } = useSelector(createUISettingsSelector());

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

  return (
    <div>
      {isSmallScreen ? <div className={styles.titleMobile}>{title}</div> : null}

      <div className={styles.header}>
        <div className={styles.navigationButtons}>
          <Button
            buttonGroupPosition="left"
            isDisabled={view === 'agenda'}
            onPress={handlePreviousPress}
          >
            <Icon name={icons.PAGE_PREVIOUS} />
          </Button>

          <Button
            buttonGroupPosition="right"
            isDisabled={view === 'agenda'}
            onPress={handleNextPress}
          >
            <Icon name={icons.PAGE_NEXT} />
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
          <div className={styles.titleDesktop}>{title}</div>
        )}

        <div className={styles.viewButtonsContainer}>
          {isFetching ? (
            <LoadingIndicator className={styles.loading} size={20} />
          ) : null}

          {isLargeScreen ? (
            <Menu className={styles.viewMenu} alignMenu={align.RIGHT}>
              <MenuButton>
                <Icon name={icons.VIEW} size={22} />
              </MenuButton>

              <MenuContent>
                {isSmallScreen ? null : (
                  <ViewMenuItem
                    name="month"
                    selectedView={view}
                    onPress={handleViewChange}
                  >
                    {translate('Month')}
                  </ViewMenuItem>
                )}

                <ViewMenuItem
                  name="week"
                  selectedView={view}
                  onPress={handleViewChange}
                >
                  {translate('Week')}
                </ViewMenuItem>

                <ViewMenuItem
                  name="forecast"
                  selectedView={view}
                  onPress={handleViewChange}
                >
                  {translate('Forecast')}
                </ViewMenuItem>

                <ViewMenuItem
                  name="day"
                  selectedView={view}
                  onPress={handleViewChange}
                >
                  {translate('Day')}
                </ViewMenuItem>

                <ViewMenuItem
                  name="agenda"
                  selectedView={view}
                  onPress={handleViewChange}
                >
                  {translate('Agenda')}
                </ViewMenuItem>
              </MenuContent>
            </Menu>
          ) : (
            <>
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
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default CalendarHeader;
