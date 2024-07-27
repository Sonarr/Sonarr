import moment from 'moment';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchTask } from 'Store/Actions/systemActions';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { isCommandExecuting } from 'Utilities/Command';
import formatDate from 'Utilities/Date/formatDate';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import styles from './ScheduledTaskRow.css';

interface ScheduledTaskRowProps {
  id: number;
  taskName: string;
  name: string;
  interval: number;
  lastExecution: string;
  lastStartTime: string;
  lastDuration: string;
  nextExecution: string;
}

function ScheduledTaskRow(props: ScheduledTaskRowProps) {
  const {
    id,
    taskName,
    name,
    interval,
    lastExecution,
    lastStartTime,
    lastDuration,
    nextExecution,
  } = props;

  const dispatch = useDispatch();

  const { showRelativeDates, longDateFormat, shortDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());
  const command = useSelector(createCommandSelector(taskName));

  const [time, setTime] = useState(Date.now());

  const isQueued = !!(command && command.status === 'queued');
  const isExecuting = isCommandExecuting(command);
  const wasExecuting = usePrevious(isExecuting);
  const isDisabled = interval === 0;
  const executeNow = !isDisabled && moment().isAfter(nextExecution);
  const hasNextExecutionTime = !isDisabled && !executeNow;
  const hasLastStartTime = moment(lastStartTime).isAfter('2010-01-01');

  const duration = useMemo(() => {
    return moment
      .duration(interval, 'minutes')
      .humanize()
      .replace(/an?(?=\s)/, '1');
  }, [interval]);

  const { lastExecutionTime, nextExecutionTime } = useMemo(() => {
    const isDisabled = interval === 0;

    if (showRelativeDates && time) {
      return {
        lastExecutionTime: moment(lastExecution).fromNow(),
        nextExecutionTime: isDisabled ? '-' : moment(nextExecution).fromNow(),
      };
    }

    return {
      lastExecutionTime: formatDate(lastExecution, shortDateFormat),
      nextExecutionTime: isDisabled
        ? '-'
        : formatDate(nextExecution, shortDateFormat),
    };
  }, [
    time,
    interval,
    lastExecution,
    nextExecution,
    showRelativeDates,
    shortDateFormat,
  ]);

  const handleExecutePress = useCallback(() => {
    dispatch(
      executeCommand({
        name: taskName,
      })
    );
  }, [taskName, dispatch]);

  useEffect(() => {
    if (!isExecuting && wasExecuting) {
      setTimeout(() => {
        dispatch(fetchTask({ id }));
      }, 1000);
    }
  }, [id, isExecuting, wasExecuting, dispatch]);

  useEffect(() => {
    const interval = setInterval(() => setTime(Date.now()), 1000);
    return () => {
      clearInterval(interval);
    };
  }, [setTime]);

  return (
    <TableRow>
      <TableRowCell>{name}</TableRowCell>
      <TableRowCell className={styles.interval}>
        {isDisabled ? 'disabled' : duration}
      </TableRowCell>

      <TableRowCell
        className={styles.lastExecution}
        title={formatDateTime(lastExecution, longDateFormat, timeFormat)}
      >
        {lastExecutionTime}
      </TableRowCell>

      {hasLastStartTime ? (
        <TableRowCell className={styles.lastDuration} title={lastDuration}>
          {formatTimeSpan(lastDuration)}
        </TableRowCell>
      ) : (
        <TableRowCell className={styles.lastDuration}>-</TableRowCell>
      )}

      {isDisabled ? (
        <TableRowCell className={styles.nextExecution}>-</TableRowCell>
      ) : null}

      {executeNow && isQueued ? (
        <TableRowCell className={styles.nextExecution}>queued</TableRowCell>
      ) : null}

      {executeNow && !isQueued ? (
        <TableRowCell className={styles.nextExecution}>now</TableRowCell>
      ) : null}

      {hasNextExecutionTime ? (
        <TableRowCell
          className={styles.nextExecution}
          title={formatDateTime(nextExecution, longDateFormat, timeFormat, {
            includeSeconds: true,
          })}
        >
          {nextExecutionTime}
        </TableRowCell>
      ) : null}

      <TableRowCell className={styles.actions}>
        <SpinnerIconButton
          name={icons.REFRESH}
          spinningName={icons.REFRESH}
          isSpinning={isExecuting}
          onPress={handleExecutePress}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default ScheduledTaskRow;
