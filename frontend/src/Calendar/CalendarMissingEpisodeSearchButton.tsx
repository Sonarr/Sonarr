import moment from 'moment-timezone';
import React, { useCallback } from 'react';
import { useQueueDetails } from 'Activity/Queue/Details/QueueDetailsProvider';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import useCalendar, {
  setCalendarSearchMissingEpisodeIds,
  useCalendarRange,
  useCalendarSearchMissingEpisodeIds,
} from './useCalendar';

const useMissingEpisodeIdsSelector = () => {
  const { start, end } = useCalendarRange();
  const { data } = useCalendar();
  const queueDetails = useQueueDetails();

  return data.reduce<number[]>((acc, episode) => {
    const airDateUtc = episode.airDateUtc;

    if (
      !episode.episodeFileId &&
      moment(airDateUtc).isAfter(start) &&
      moment(airDateUtc).isBefore(end) &&
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
  const searchMissingEpisodeIds = useCalendarSearchMissingEpisodeIds();
  const missingEpisodeIds = useMissingEpisodeIdsSelector();

  const isSearchingEpisodes = useCommandExecuting(CommandNames.EpisodeSearch, {
    episodeIds: searchMissingEpisodeIds,
  });

  const isSearchingForMissing =
    !!searchMissingEpisodeIds && isSearchingEpisodes;

  const handlePress = useCallback(() => {
    setCalendarSearchMissingEpisodeIds(missingEpisodeIds);

    executeCommand({
      name: CommandNames.EpisodeSearch,
      episodeIds: missingEpisodeIds,
    });
  }, [missingEpisodeIds, executeCommand]);

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
