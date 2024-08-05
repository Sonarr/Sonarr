import moment from 'moment';
import React, { useEffect, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';

interface StartTimeProps {
  startTime: string;
}

function StartTime(props: StartTimeProps) {
  const { startTime } = props;
  const { timeFormat, longDateFormat } = useSelector(
    createUISettingsSelector()
  );
  const [time, setTime] = useState(Date.now());

  const { formattedStartTime, uptime } = useMemo(() => {
    return {
      uptime: formatTimeSpan(moment(time).diff(startTime)),
      formattedStartTime: formatDateTime(
        startTime,
        longDateFormat,
        timeFormat,
        {
          includeSeconds: true,
        }
      ),
    };
  }, [startTime, time, longDateFormat, timeFormat]);

  useEffect(() => {
    const interval = setInterval(() => setTime(Date.now()), 1000);

    return () => {
      clearInterval(interval);
    };
  }, [setTime]);

  return <span title={formattedStartTime}>{uptime}</span>;
}

export default StartTime;
