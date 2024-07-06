import React from 'react';
import { useSelector } from 'react-redux';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import createSeriesQueueItemsDetailsSelector, {
  SeriesQueueDetails,
} from 'Series/Index/createSeriesQueueDetailsSelector';

function getEpisodeCountKind(
  monitored: boolean,
  episodeFileCount: number,
  episodeCount: number,
  isDownloading: boolean
) {
  if (isDownloading) {
    return kinds.PURPLE;
  }

  if (episodeFileCount === episodeCount && episodeCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

interface SeasonProgressLabelProps {
  seriesId: number;
  seasonNumber: number;
  monitored: boolean;
  episodeCount: number;
  episodeFileCount: number;
}

function SeasonProgressLabel({
  seriesId,
  seasonNumber,
  monitored,
  episodeCount,
  episodeFileCount,
}: SeasonProgressLabelProps) {
  const queueDetails: SeriesQueueDetails = useSelector(
    createSeriesQueueItemsDetailsSelector(seriesId, seasonNumber)
  );

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const text = newDownloads
    ? `${episodeFileCount} + ${newDownloads} / ${episodeCount}`
    : `${episodeFileCount} / ${episodeCount}`;

  return (
    <Label
      kind={getEpisodeCountKind(
        monitored,
        episodeFileCount,
        episodeCount,
        queueDetails.count > 0
      )}
      size={sizes.LARGE}
    >
      <span>{text}</span>
    </Label>
  );
}

export default SeasonProgressLabel;
