import moment from 'moment';
import React from 'react';
import { useSelector } from 'react-redux';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import translate from 'Utilities/String/translate';

interface EpisodeAiringProps {
  airDateUtc?: string;
  network: string;
}

function EpisodeAiring(props: EpisodeAiringProps) {
  const { airDateUtc, network } = props;

  const { shortDateFormat, showRelativeDates, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  const networkLabel = (
    <Label kind={kinds.INFO} size={sizes.MEDIUM}>
      {network}
    </Label>
  );

  // TODO: Update InlineMarkdown to accept tags and pass in networkLabel object, for now blank string passed into translation
  if (!airDateUtc) {
    return (
      <span>
        {translate('AirsTbaOn', { networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  const time = formatTime(airDateUtc, timeFormat);

  if (!showRelativeDates) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', {
          date: moment(airDateUtc).format(shortDateFormat),
          time,
          networkLabel: '',
        })}
        {networkLabel}
      </span>
    );
  }

  if (isToday(airDateUtc)) {
    return (
      <span>
        {translate('AirsTimeOn', { time, networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  if (isTomorrow(airDateUtc)) {
    return (
      <span>
        {translate('AirsTomorrowOn', { time, networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  if (isInNextWeek(airDateUtc)) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', {
          date: moment(airDateUtc).format('dddd'),
          time,
          networkLabel: '',
        })}
        {networkLabel}
      </span>
    );
  }

  return (
    <span>
      {translate('AirsDateAtTimeOn', {
        date: moment(airDateUtc).format(shortDateFormat),
        time,
        networkLabel: '',
      })}
      {networkLabel}
    </span>
  );
}

export default EpisodeAiring;
