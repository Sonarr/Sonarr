import { useContext, useMemo } from 'react';
import Column from 'Components/Table/Column';
import Episode from 'Episode/Episode';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';
import { EpisodeFileContext } from 'EpisodeFile/EpisodeFileProvider';
import { SeriesType } from 'Series/Series';

const SIZER_ROWS_PER_COLUMN = 2;
const WARNING_WIDTH = 22;
const FINALE_TYPE_WIDTH = 100;

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

type MeasureText = (text: string | null | undefined) => number;

type GetContentWidth = (
  episode: Episode,
  episodeFile: EpisodeFile | undefined,
  measureText: MeasureText,
  seriesType: SeriesType
) => number;

function createTextMeasurer(): MeasureText {
  const context = document.createElement('canvas').getContext('2d');

  if (context) {
    context.font = getComputedStyle(document.body).font;
  }

  return (text) => {
    if (!text) {
      return 0;
    }

    return context ? context.measureText(text).width : text.length;
  };
}

const CONTENT_WIDTHS: Record<string, GetContentWidth> = {
  episodeNumber: (
    { episodeNumber, absoluteEpisodeNumber, unverifiedSceneNumbering },
    _,
    measureText,
    seriesType
  ) => {
    const isAnime = seriesType === 'anime';
    const absolute = isAnime && absoluteEpisodeNumber;
    const hasWarning = unverifiedSceneNumbering || (isAnime && !absolute);

    return (
      measureText(`${episodeNumber}${absolute ? ` (${absolute})` : ''}`) +
      (hasWarning ? WARNING_WIDTH : 0)
    );
  },
  title: ({ title, finaleType }, _, measureText) =>
    measureText(title) + (finaleType ? FINALE_TYPE_WIDTH : 0),
  path: (_, file, measureText) => measureText(file?.path),
  relativePath: (_, file, measureText) => measureText(file?.relativePath),
  languages: (_, file, measureText) =>
    measureText(file?.languages?.map(({ name }) => name).join()),
  audioInfo: (_, file, measureText) =>
    measureText(file?.mediaInfo?.audioStreams?.[0]?.codec),
  audioLanguages: (_, file) => file?.mediaInfo?.audioStreams?.length ?? 0,
  subtitleLanguages: (_, file) => file?.mediaInfo?.subtitleStreams?.length ?? 0,
  videoCodec: (_, file, measureText) =>
    measureText(file?.mediaInfo?.videoCodec),
  videoDynamicRangeType: (_, file, measureText) =>
    measureText(file?.mediaInfo?.videoDynamicRangeType),
  status: (_, file, measureText) => measureText(file?.quality.quality.name),
  releaseGroup: (_, file, measureText) => measureText(file?.releaseGroup),
  customFormats: (_, file, measureText) =>
    measureText(file?.customFormats?.map(({ name }) => name).join()),
};

function getLatestPerMonth(episodes: Episode[]) {
  const latest = new Map<string, Episode>();

  episodes.forEach((episode) => {
    if (!episode.airDate) {
      return;
    }

    const month = episode.airDate.slice(5, 7);
    const current = latest.get(month);

    if (!current || episode.airDate.slice(8) > current.airDate.slice(8)) {
      latest.set(month, episode);
    }
  });

  return [...latest.values()];
}

function getSizerEpisodes(
  episodes: Episode[],
  episodeFiles: ReadonlyArray<EpisodeFile>,
  columns: Column[],
  seriesType: SeriesType
) {
  const filesById = new Map(episodeFiles.map((file) => [file.id, file]));
  const measureText = createTextMeasurer();
  const sizerEpisodes = new Map<Episode, string[]>();

  const addSizerEpisode = (episode: Episode, columnName: string) => {
    sizerEpisodes.set(episode, [
      ...(sizerEpisodes.get(episode) ?? []),
      columnName,
    ]);
  };

  columns.forEach(({ name, isVisible }) => {
    if (!isVisible) {
      return;
    }

    if (name === 'airDateUtc') {
      getLatestPerMonth(episodes).forEach((episode) =>
        addSizerEpisode(episode, name)
      );

      return;
    }

    const getContentWidth = CONTENT_WIDTHS[name];

    if (!getContentWidth) {
      return;
    }

    episodes
      .map((episode) => ({
        episode,
        width: getContentWidth(
          episode,
          filesById.get(episode.episodeFileId),
          measureText,
          seriesType
        ),
      }))
      .sort((a, b) => b.width - a.width)
      .slice(0, SIZER_ROWS_PER_COLUMN)
      .forEach(({ episode }) => addSizerEpisode(episode, name));
  });

  return [...sizerEpisodes].map(([episode, sizerColumns]) => ({
    episode,
    sizerColumns,
  }));
}

function useEpisodeTableLayout(
  episodes: Episode[],
  seriesType: SeriesType,
  columns: Column[],
  isVirtual: boolean
) {
  const episodeFiles = useContext(EpisodeFileContext);

  const gridTemplateColumns = useMemo(() => {
    const visibleColumns = columns.filter(({ isVisible }) => isVisible);
    const hasTitle = visibleColumns.some(({ name }) => name === 'title');

    return visibleColumns
      .map(({ name }) => getColumnTrack(name, seriesType, hasTitle))
      .join(' ');
  }, [columns, seriesType]);

  const sizerEpisodes = useMemo(() => {
    return isVirtual
      ? getSizerEpisodes(episodes, episodeFiles ?? [], columns, seriesType)
      : [];
  }, [columns, episodeFiles, episodes, isVirtual, seriesType]);

  return { gridTemplateColumns, sizerEpisodes };
}

export default useEpisodeTableLayout;
