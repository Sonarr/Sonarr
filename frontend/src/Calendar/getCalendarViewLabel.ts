import translate from 'Utilities/String/translate';
import { CalendarView } from './calendarViews';

const viewTranslationKeys: Record<CalendarView, string> = {
  agenda: 'Agenda',
  day: 'Day',
  forecast: 'Forecast',
  month: 'Month',
  week: 'Week',
};

export default function getCalendarViewLabel(view: CalendarView) {
  return translate(viewTranslationKeys[view]);
}
