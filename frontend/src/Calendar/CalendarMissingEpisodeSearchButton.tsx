import moment from 'moment';
import React, { useCallback } from 'react';
import { useQueueDetails } from 'Activity/Queue/Details/QueueDetailsProvider';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import useCalendar, { useCalendarRange } from './useCalendar';

const useMissingEpisodeIdsSelector = () => {
  const { start, end } = useCalendarRange();
  const { data } = useCalendar();
  const queueDetails = useQueueDetails();
  const rangeStart = moment(start).startOf('day');
  const rangeEnd = moment(end).endOf('day');

  return data.reduce<number[]>((acc, episode) => {
    const airDateUtc = episode.airDateUtc;

    if (
      !episode.episodeFileId &&
      moment(airDateUtc).isBetween(rangeStart, rangeEnd, undefined, '[]') &&
      isBefore(episode.airDateUtc) &&
      !queueDetails.some((details) => details.episodeIds?.includes(episode.id))
    ) {
      acc.push(episode.id);
    }

    return acc;
  }, []);
};

export default function CalendarMissingEpisodeSearchButton() {
  const executeCommand = useExecuteCommand();
  const missingEpisodeIds = useMissingEpisodeIdsSelector();
  const isSearchingForMissing = useCommandExecuting(
    CommandNames.EpisodeSearch,
    {
      episodeIds: missingEpisodeIds,
    }
  );

  const handlePress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      episodeIds: missingEpisodeIds,
    });
  }, [executeCommand, missingEpisodeIds]);

  return (
    <PageToolbarButton
      label={translate('SearchForMissing')}
      iconName={icons.SEARCH}
      isDisabled={!missingEpisodeIds.length}
      isSpinning={isSearchingForMissing}
      onPress={handlePress}
    />
  );
}
