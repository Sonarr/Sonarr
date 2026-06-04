import {
  autoUpdate,
  flip,
  FloatingPortal,
  shift,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import moment from 'moment';
import React, { useCallback, useMemo, useState } from 'react';
import { useAppDimensions } from 'App/appStore';
import {
  setCalendarOption,
  useCalendarOption,
} from 'Calendar/calendarOptionsStore';
import { CalendarView } from 'Calendar/calendarViews';
import useCalendar, {
  goToDate,
  goToNextRange,
  goToPreviousRange,
  goToToday,
  useCalendarRange,
  useCalendarTime,
} from 'Calendar/useCalendar';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import ViewMenuItem from 'Components/Menu/ViewMenuItem';
import { align, icons } from 'Helpers/Props';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import translate from 'Utilities/String/translate';
import CalendarHeaderViewButton from './CalendarHeaderViewButton';
import styles from './CalendarHeader.css';

const MONTH_NAMES = [
  'Jan',
  'Feb',
  'Mar',
  'Apr',
  'May',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Oct',
  'Nov',
  'Dec',
];

interface MonthButtonProps {
  name: string;
  index: number;
  isActive: boolean;
  onPress: (month: number) => void;
}

function MonthButton({ name, index, isActive, onPress }: MonthButtonProps) {
  const handlePress = useCallback(() => {
    onPress(index);
  }, [index, onPress]);

  return (
    <Link
      className={isActive ? styles.monthButtonActive : styles.monthButton}
      onPress={handlePress}
    >
      {name}
    </Link>
  );
}

function CalendarHeader() {
  const { isFetching } = useCalendar();
  const view = useCalendarOption('view');
  const time = useCalendarTime();
  const { start, end } = useCalendarRange();

  const { isSmallScreen, isLargeScreen } = useAppDimensions();

  const { longDateFormat } = useUiSettingsValues();

  const [isMonthPickerOpen, setIsMonthPickerOpen] = useState(false);
  const [pickerYear, setPickerYear] = useState(() => moment(time).year());

  const currentMonth = moment(time).month();
  const currentYear = moment(time).year();

  const { refs, context, floatingStyles } = useFloating({
    middleware: [flip({ crossAxis: false, mainAxis: true }), shift()],
    open: isMonthPickerOpen,
    placement: 'bottom',
    whileElementsMounted: autoUpdate,
    onOpenChange: setIsMonthPickerOpen,
  });

  const dismiss = useDismiss(context, { outsidePressEvent: 'click' });
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss]);

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

  const handleTitlePress = useCallback(() => {
    if (view === 'agenda') {
      return;
    }

    setPickerYear(moment(time).year());
    setIsMonthPickerOpen((open) => !open);
  }, [view, time]);

  const handlePickerPrevYear = useCallback(() => {
    setPickerYear((y) => y - 1);
  }, []);

  const handlePickerNextYear = useCallback(() => {
    setPickerYear((y) => y + 1);
  }, []);

  const handleMonthSelect = useCallback(
    (month: number) => {
      goToDate(moment({ year: pickerYear, month, day: 1 }));
      setIsMonthPickerOpen(false);
    },
    [pickerYear]
  );

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

  const titleContent = (
    <>
      {title}
      {view === 'agenda' ? null : (
        <Icon
          className={styles.titleChevron}
          name={icons.CARET_DOWN}
          size={14}
        />
      )}
    </>
  );

  const monthPicker = (
    <div className={styles.monthPicker}>
      <div className={styles.monthPickerYear}>
        <Link onPress={handlePickerPrevYear}>
          <Icon name={icons.PAGE_PREVIOUS} size={14} />
        </Link>

        <span>{pickerYear}</span>

        <Link onPress={handlePickerNextYear}>
          <Icon name={icons.PAGE_NEXT} size={14} />
        </Link>
      </div>

      <div className={styles.monthGrid}>
        {MONTH_NAMES.map((name, index) => (
          <MonthButton
            key={name}
            name={name}
            index={index}
            isActive={index === currentMonth && pickerYear === currentYear}
            onPress={handleMonthSelect}
          />
        ))}
      </div>
    </div>
  );

  return (
    <div>
      {isSmallScreen ? (
        <div
          ref={refs.setReference}
          className={styles.titleMobile}
          {...getReferenceProps()}
        >
          <Link
            className={styles.titleLink}
            isDisabled={view === 'agenda'}
            onPress={handleTitlePress}
          >
            {titleContent}
          </Link>
        </div>
      ) : null}

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
          <div
            ref={refs.setReference}
            className={styles.titleDesktop}
            {...getReferenceProps()}
          >
            <Link
              className={styles.titleLink}
              isDisabled={view === 'agenda'}
              onPress={handleTitlePress}
            >
              {titleContent}
            </Link>
          </div>
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

      {isMonthPickerOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            style={floatingStyles}
            className={styles.monthPickerPortal}
            {...getFloatingProps()}
          >
            {monthPicker}
          </div>
        </FloatingPortal>
      ) : null}
    </div>
  );
}

export default CalendarHeader;
