import moment from 'moment';
import translate from 'Utilities/String/translate';

function formatShortTimeSpan(timeSpan) {
  if (!timeSpan) {
    return '';
  }

  const duration = moment.duration(timeSpan);

  const hours = Math.floor(duration.asHours());
  const minutes = Math.floor(duration.asMinutes());
  const seconds = Math.floor(duration.asSeconds());

  if (hours > 0) {
    return translate('FormatShortTimeSpanHours', { hours });
  }

  if (minutes > 0) {
    return translate('FormatShortTimeSpanMinutes', { minutes });
  }

  return translate('FormatShortTimeSpanSeconds', { seconds });
}

export default formatShortTimeSpan;
