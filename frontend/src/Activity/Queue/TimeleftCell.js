import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import formatTime from 'Utilities/Date/formatTime';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './TimeleftCell.css';

function TimeleftCell(props) {
  const {
    estimatedCompletionTime,
    timeleft,
    status,
    size,
    sizeleft,
    showRelativeDates,
    shortDateFormat,
    timeFormat
  } = props;

  if (status === 'delay') {
    const date = getRelativeDate({
      date: estimatedCompletionTime,
      shortDateFormat,
      showRelativeDates
    });
    const time = formatTime(estimatedCompletionTime, timeFormat, { includeMinuteZero: true });

    return (
      <TableRowCell className={styles.timeleft}>
        <Tooltip
          anchor={<Icon name={icons.INFO} />}
          tooltip={translate('DelayingDownloadUntil', { date, time })}
          kind={kinds.INVERSE}
          position={tooltipPositions.TOP}
        />
      </TableRowCell>
    );
  }

  if (status === 'downloadClientUnavailable') {
    const date = getRelativeDate({
      date: estimatedCompletionTime,
      shortDateFormat,
      showRelativeDates
    });
    const time = formatTime(estimatedCompletionTime, timeFormat, { includeMinuteZero: true });

    return (
      <TableRowCell className={styles.timeleft}>
        <Tooltip
          anchor={<Icon name={icons.INFO} />}
          tooltip={translate('RetryingDownloadOn', { date, time })}
          kind={kinds.INVERSE}
          position={tooltipPositions.TOP}
        />
      </TableRowCell>
    );
  }

  if (!timeleft || status === 'completed' || status === 'failed') {
    return (
      <TableRowCell className={styles.timeleft}>
        -
      </TableRowCell>
    );
  }

  const totalSize = formatBytes(size);
  const remainingSize = formatBytes(sizeleft);

  return (
    <TableRowCell
      className={styles.timeleft}
      title={`${remainingSize} / ${totalSize}`}
    >
      {formatTimeSpan(timeleft)}
    </TableRowCell>
  );
}

TimeleftCell.propTypes = {
  estimatedCompletionTime: PropTypes.string,
  timeleft: PropTypes.string,
  status: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  sizeleft: PropTypes.number.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default TimeleftCell;
