import { kinds } from 'Helpers/Props';

function getProgressBarKind(status, monitored, progress) {
  if (progress === 100) {
    return status === 'ended' ? kinds.SUCCESS : kinds.PRIMARY;
  }

  if (monitored) {
    return kinds.DANGER;
  }

  return kinds.WARNING;
}

export default getProgressBarKind;
