import React from 'react';
import { useQueueDetailsForSeries } from 'Activity/Queue/Details/QueueDetailsProvider';
import Label from 'Components/Label';
import { icons, kinds, sizes } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';

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

function getEpisodeCountIcon(kind: Kind) {
  switch (kind) {
    case kinds.PURPLE:
      return icons.DOWNLOADING;
    case kinds.SUCCESS:
      return icons.CHECK;
    case kinds.WARNING:
      return icons.UNMONITORED;
    default:
      return icons.MISSING;
  }
}

interface SeriesProgressLabelProps {
  className: string;
  seriesId: number;
  monitored: boolean;
  episodeCount: number;
  episodeFileCount: number;
}

function SeriesProgressLabel({
  className,
  seriesId,
  monitored,
  episodeCount,
  episodeFileCount,
}: SeriesProgressLabelProps) {
  const queueDetails = useQueueDetailsForSeries(seriesId);

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const text = newDownloads
    ? `${episodeFileCount} + ${newDownloads} / ${episodeCount}`
    : `${episodeFileCount} / ${episodeCount}`;

  const kind = getEpisodeCountKind(
    monitored,
    episodeFileCount,
    episodeCount,
    queueDetails.count > 0
  );

  return (
    <Label
      className={className}
      kind={kind}
      size={sizes.LARGE}
      icon={getEpisodeCountIcon(kind)}
    >
      <span>{text}</span>
    </Label>
  );
}

export default SeriesProgressLabel;
