import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import formatDate from 'Utilities/Date/formatDate';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import styles from './ScheduledTaskRow.css';

function getFormattedDates(props) {
  const {
    lastExecution,
    nextExecution,
    interval,
    showRelativeDates,
    shortDateFormat
  } = props;

  const isDisabled = interval === 0;

  if (showRelativeDates) {
    return {
      lastExecutionTime: moment(lastExecution).fromNow(),
      nextExecutionTime: isDisabled ? '-' : moment(nextExecution).fromNow()
    };
  }

  return {
    lastExecutionTime: formatDate(lastExecution, shortDateFormat),
    nextExecutionTime: isDisabled ? '-' : formatDate(nextExecution, shortDateFormat)
  };
}

class ScheduledTaskRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = getFormattedDates(props);

    this._updateTimeoutId = null;
  }

  componentDidMount() {
    this.setUpdateTimer();
  }

  componentDidUpdate(prevProps) {
    const {
      lastExecution,
      nextExecution
    } = this.props;

    if (
      lastExecution !== prevProps.lastExecution ||
      nextExecution !== prevProps.nextExecution
    ) {
      this.setState(getFormattedDates(this.props));
    }
  }

  componentWillUnmount() {
    if (this._updateTimeoutId) {
      this._updateTimeoutId = clearTimeout(this._updateTimeoutId);
    }
  }

  //
  // Listeners

  setUpdateTimer() {
    const { interval } = this.props;
    const timeout = interval < 60 ? 10000 : 60000;

    this._updateTimeoutId = setTimeout(() => {
      this.setState(getFormattedDates(this.props));
      this.setUpdateTimer();
    }, timeout);
  }

  //
  // Render

  render() {
    const {
      name,
      interval,
      lastExecution,
      lastStartTime,
      lastDuration,
      nextExecution,
      isQueued,
      isExecuting,
      longDateFormat,
      timeFormat,
      onExecutePress
    } = this.props;

    const {
      lastExecutionTime,
      nextExecutionTime
    } = this.state;

    const isDisabled = interval === 0;
    const executeNow = !isDisabled && moment().isAfter(nextExecution);
    const hasNextExecutionTime = !isDisabled && !executeNow;
    const duration = moment.duration(interval, 'minutes').humanize().replace(/an?(?=\s)/, '1');
    const hasLastStartTime = moment(lastStartTime).isAfter('2010-01-01');

    return (
      <TableRow>
        <TableRowCell>{name}</TableRowCell>
        <TableRowCell
          className={styles.interval}
        >
          {isDisabled ? 'disabled' : duration}
        </TableRowCell>

        <TableRowCell
          className={styles.lastExecution}
          title={formatDateTime(lastExecution, longDateFormat, timeFormat)}
        >
          {lastExecutionTime}
        </TableRowCell>

        {
          !hasLastStartTime &&
            <TableRowCell className={styles.lastDuration}>-</TableRowCell>
        }

        {
          hasLastStartTime &&
            <TableRowCell
              className={styles.lastDuration}
              title={lastDuration}
            >
              {formatTimeSpan(lastDuration)}
            </TableRowCell>
        }

        {
          isDisabled &&
            <TableRowCell className={styles.nextExecution}>-</TableRowCell>
        }

        {
          executeNow && isQueued &&
            <TableRowCell className={styles.nextExecution}>queued</TableRowCell>
        }

        {
          executeNow && !isQueued &&
            <TableRowCell className={styles.nextExecution}>now</TableRowCell>
        }

        {
          hasNextExecutionTime &&
            <TableRowCell
              className={styles.nextExecution}
              title={formatDateTime(nextExecution, longDateFormat, timeFormat, { includeSeconds: true })}
            >
              {nextExecutionTime}
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
}

ScheduledTaskRow.propTypes = {
  name: PropTypes.string.isRequired,
  interval: PropTypes.number.isRequired,
  lastExecution: PropTypes.string.isRequired,
  lastStartTime: PropTypes.string.isRequired,
  lastDuration: PropTypes.string.isRequired,
  nextExecution: PropTypes.string.isRequired,
  isQueued: PropTypes.bool.isRequired,
  isExecuting: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onExecutePress: PropTypes.func.isRequired
};

export default ScheduledTaskRow;
