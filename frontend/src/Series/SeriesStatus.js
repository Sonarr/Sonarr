
import { icons } from 'Helpers/Props';

export function getSeriesStatusDetails(status) {

  let statusDetails = {
    icon: icons.SERIES_CONTINUING,
    title: 'Continuing',
    message: 'More episodes/another season is expected'
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: 'Deleted',
      message: 'Series was deleted from TheTVDB'
    };
  } else if (status === 'ended') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: 'Ended',
      message: 'No additional episodes or or another season is expected'
    };
  }

  return statusDetails;
}
