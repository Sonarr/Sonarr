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

  if (episodeCount === 0) {
    return kinds.DEFAULT;
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
    case kinds.DEFAULT:
      return icons.NOT_AIRED;
    default:
      return icons.MISSING;
  }
}

interface SeasonProgressLabelProps {
  className: string;
  seriesId: number;
  seasonNumber: number;
  monitored: boolean;
  episodeCount: number;
  episodeFileCount: number;
}

function SeasonProgressLabel({
  className,
  seriesId,
  seasonNumber,
  monitored,
  episodeCount,
  episodeFileCount,
}: SeasonProgressLabelProps) {
  const queueDetails = useQueueDetailsForSeries(seriesId, seasonNumber);

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

export default SeasonProgressLabel;
