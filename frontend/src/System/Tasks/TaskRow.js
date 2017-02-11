import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import formatDate from 'Utilities/Date/formatDate';
import formatDateTime from 'Utilities/Date/formatDateTime';
import { icons } from 'Helpers/Props';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './TaskRow.css';

function TaskRow(props) {
  const {
    name,
    interval,
    lastExecution,
    nextExecution,
    isExecuting,
    showRelativeDates,
    shortDateFormat,
    longDateFormat,
    timeFormat,
    onExecutePress
  } = props;

  const disabled = interval === 0;
  const executeNow = !disabled && moment().isAfter(nextExecution);
  const hasNextExecutionTime = !disabled && !executeNow;
  const duration = moment.duration(interval, 'minutes').humanize().replace(/an?(?=\s)/, '1');

  return (
    <TableRow>
      <TableRowCell>{name}</TableRowCell>
      <TableRowCell
        className={styles.interval}
      >
        {disabled ? 'disabled' : duration}
      </TableRowCell>

      <TableRowCell
        className={styles.lastExecution}
        title={formatDateTime(lastExecution, longDateFormat, timeFormat)}
      >
        {showRelativeDates ? moment(lastExecution).fromNow() : formatDate(lastExecution, shortDateFormat)}
      </TableRowCell>

      {
        disabled &&
          <TableRowCell className={styles.nextExecution}>-</TableRowCell>
      }

      {
        executeNow &&
          <TableRowCell className={styles.nextExecution}>now</TableRowCell>
      }

      {
        hasNextExecutionTime &&
          <TableRowCell
            className={styles.nextExecution}
            title={formatDateTime(nextExecution, longDateFormat, timeFormat, { includeSeconds: true })}
          >
            {showRelativeDates ? moment(nextExecution).fromNow() : formatDate(nextExecution, shortDateFormat)}
          </TableRowCell>
      }

      <TableRowCell
        className={styles.actions}
      >
        <SpinnerIconButton
          name={icons.REFRESH}
          spinningName={icons.REFRESH}
          isSpinning={isExecuting}
          onPress={onExecutePress}
        />
      </TableRowCell>
    </TableRow>
  );
}

TaskRow.propTypes = {
  name: PropTypes.string.isRequired,
  interval: PropTypes.number.isRequired,
  lastExecution: PropTypes.string.isRequired,
  nextExecution: PropTypes.string.isRequired,
  isExecuting: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onExecutePress: PropTypes.func.isRequired
};

export default TaskRow;
