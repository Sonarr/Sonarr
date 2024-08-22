import { icons } from 'Helpers/Props';
import { SeriesStatus } from 'Series/Series';
import translate from 'Utilities/String/translate';

export function getSeriesStatusDetails(status: SeriesStatus) {
  let statusDetails = {
    icon: icons.SERIES_CONTINUING,
    title: translate('Continuing'),
    message: translate('ContinuingSeriesDescription'),
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedSeriesDescription'),
    };
  } else if (status === 'ended') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: translate('Ended'),
      message: translate('EndedSeriesDescription'),
    };
  } else if (status === 'upcoming') {
    statusDetails = {
      icon: icons.SERIES_CONTINUING,
      title: translate('Upcoming'),
      message: translate('UpcomingSeriesDescription'),
    };
  }

  return statusDetails;
}
