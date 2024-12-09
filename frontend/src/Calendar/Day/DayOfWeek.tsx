import classNames from 'classnames';
import moment from 'moment';
import React from 'react';
import * as calendarViews from 'Calendar/calendarViews';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './DayOfWeek.css';

interface DayOfWeekProps {
  date: string;
  view: string;
  isTodaysDate: boolean;
  calendarWeekColumnHeader: string;
  shortDateFormat: string;
  showRelativeDates: boolean;
}

function DayOfWeek(props: DayOfWeekProps) {
  const {
    date,
    view,
    isTodaysDate,
    calendarWeekColumnHeader,
    shortDateFormat,
    showRelativeDates,
  } = props;

  const highlightToday = view !== calendarViews.MONTH && isTodaysDate;
  const momentDate = moment(date);
  let formatedDate = momentDate.format('dddd');

  if (view === calendarViews.WEEK) {
    formatedDate = momentDate.format(calendarWeekColumnHeader);
  } else if (view === calendarViews.FORECAST) {
    formatedDate = getRelativeDate({
      date,
      shortDateFormat,
      showRelativeDates,
    });
  }

  return (
    <div
      className={classNames(
        styles.dayOfWeek,
        view === calendarViews.DAY && styles.isSingleDay,
        highlightToday && styles.isToday
      )}
    >
      {formatedDate}
    </div>
  );
}

export default DayOfWeek;
