import { CalendarStatus } from 'typings/Calendar';
import translate from 'Utilities/String/translate';

const statusTranslationKeys: Record<CalendarStatus, string> = {
  downloaded: 'CalendarLegendEpisodeDownloadedTooltip',
  downloading: 'CalendarLegendEpisodeDownloadingTooltip',
  unmonitored: 'CalendarLegendEpisodeUnmonitoredTooltip',
  onAir: 'CalendarLegendEpisodeOnAirTooltip',
  missing: 'CalendarLegendEpisodeMissingTooltip',
  unaired: 'CalendarLegendEpisodeUnairedTooltip',
};

function getCalendarStatusLabel(status: CalendarStatus) {
  return translate(statusTranslationKeys[status]);
}

export default getCalendarStatusLabel;
