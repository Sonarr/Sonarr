import { useMemo } from 'react';
import Column from 'Components/Table/Column';
import { SeriesType } from 'Series/Series';

const COLUMN_WIDTHS: Record<string, number> = {
  monitored: 42,
  episodeNumber: 65,
  airDateUtc: 180,
  runtime: 80,
  languages: 100,
  audioInfo: 100,
  videoCodec: 100,
  size: 100,
  status: 100,
  audioLanguages: 165,
  subtitleLanguages: 165,
  videoDynamicRangeType: 165,
  releaseGroup: 120,
  customFormatScore: 55,
  indexerFlags: 50,
  actions: 70,
};

const ANIME_NUMBER_WIDTH = 95;
const FIXED_COLUMNS = ['monitored', 'episodeNumber', 'actions'];

function getColumnTrack(
  name: string,
  seriesType: SeriesType,
  hasTitle: boolean
) {
  if (name === 'title') {
    return 'minmax(max-content, 1fr)';
  }

  const width =
    name === 'episodeNumber' && seriesType === 'anime'
      ? ANIME_NUMBER_WIDTH
      : COLUMN_WIDTHS[name];

  if (!width) {
    return 'max-content';
  }

  return !hasTitle && !FIXED_COLUMNS.includes(name)
    ? `minmax(min-content, ${width}fr)`
    : `minmax(min-content, ${width}px)`;
}

function useEpisodeTableLayout(seriesType: SeriesType, columns: Column[]) {
  return useMemo(() => {
    const visibleColumns = columns.filter(({ isVisible }) => isVisible);
    const hasTitle = visibleColumns.some(({ name }) => name === 'title');

    return visibleColumns
      .map(({ name }) => getColumnTrack(name, seriesType, hasTitle))
      .join(' ');
  }, [columns, seriesType]);
}

export default useEpisodeTableLayout;
