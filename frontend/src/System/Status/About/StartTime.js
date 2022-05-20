import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';

function getUptime(startTime) {
  return formatTimeSpan(moment().diff(startTime));
}

class StartTime extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      startTime,
      timeFormat,
      longDateFormat
    } = props;

    this._timeoutId = null;

    this.state = {
      uptime: getUptime(startTime),
      startTime: formatDateTime(startTime, longDateFormat, timeFormat, { includeSeconds: true })
    };
  }

  componentDidMount() {
    this._timeoutId = setTimeout(this.onTimeout, 1000);
  }

  componentDidUpdate(prevProps) {
    const {
      startTime,
      timeFormat,
      longDateFormat
    } = this.props;

    if (
      startTime !== prevProps.startTime ||
      timeFormat !== prevProps.timeFormat ||
      longDateFormat !== prevProps.longDateFormat
    ) {
      this.setState({
        uptime: getUptime(startTime),
        startTime: formatDateTime(startTime, longDateFormat, timeFormat, { includeSeconds: true })
      });
    }
  }

  componentWillUnmount() {
    if (this._timeoutId) {
      this._timeoutId = clearTimeout(this._timeoutId);
    }
  }

  //
  // Listeners

  onTimeout = () => {
    this.setState({ uptime: getUptime(this.props.startTime) });
    this._timeoutId = setTimeout(this.onTimeout, 1000);
  };

  //
  // Render

  render() {
    const {
      uptime,
      startTime
    } = this.state;

    return (
      <span title={startTime}>
        {uptime}
      </span>
    );
  }
}

StartTime.propTypes = {
  startTime: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

export default StartTime;
