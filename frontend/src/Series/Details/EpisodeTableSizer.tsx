import React, { memo, ReactNode, useContext } from 'react';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import Column from 'Components/Table/Column';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import Episode from 'Episode/Episode';
import { getWarningMessage } from 'Episode/EpisodeNumber';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';
import { EpisodeFileContext } from 'EpisodeFile/EpisodeFileProvider';
import { SeriesType } from 'Series/Series';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import isBefore from 'Utilities/Date/isBefore';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import formatRuntime from 'Utilities/Number/formatRuntime';
import EpisodeRow from './EpisodeRow';
import rowStyles from './EpisodeRow.module.css';
import styles from './SeriesDetailsSeason.module.css';

type Get<T = unknown> = (episode: Episode, file?: EpisodeFile) => T;
type GetText = Get<string | undefined>;

const LINES: Record<string, GetText> = {
  path: (_, file) => file?.path,
  relativePath: (_, file) => file?.relativePath,
  releaseGroup: (_, file) => file?.releaseGroup,
  runtime: ({ runtime }) => formatRuntime(runtime),
  size: (_, file) => (file?.size ? formatBytes(file.size) : undefined),
};

const SAMPLES: Record<string, Get> = {
  languages: (_, file) => {
    const languages = file?.languages ?? [];

    return languages.length > 1 || languages[0]?.name;
  },
  customFormats: (_, file) => file?.customFormats.map(({ name }) => name),
  customFormatScore: (_, file) =>
    file &&
    formatCustomFormatScore(file.customFormatScore, file.customFormats.length),
  audioInfo: (_, file) => {
    const stream = file?.mediaInfo?.audioStreams?.[0];

    return stream && [stream.codec, stream.channels];
  },
  audioLanguages: (_, file) =>
    file?.mediaInfo?.audioStreams?.map(({ language }) => language),
  subtitleLanguages: (_, file) =>
    file?.mediaInfo?.subtitleStreams?.map(({ language }) => language),
  videoCodec: (_, file) => file?.mediaInfo?.videoCodec,
  videoDynamicRangeType: (_, file) => file?.mediaInfo?.videoDynamicRangeType,
  indexerFlags: (_, file) => !!file?.indexerFlags,
  status: ({ grabbed, monitored, airDateUtc }, file) => {
    if (grabbed) {
      return 'grabbed';
    }

    return file
      ? [file.quality, file.qualityCutoffNotMet]
      : [monitored, !!airDateUtc, isBefore(airDateUtc)];
  },
};

function hasBreak(value?: string) {
  return !!value && /[\n\r\f]/.test(value);
}

function toLines(values: (string | undefined)[]) {
  return [...new Set(values.filter((v): v is string => !!v && !hasBreak(v)))]
    .sort((a, b) => b.length - a.length)
    .join('\n');
}

interface EpisodeTableSizerProps {
  seriesId: number;
  seriesType: SeriesType;
  columns: Column[];
  episodes: Episode[];
  onMonitorEpisodePress: (
    episodeId: number,
    value: boolean,
    { shiftKey }: { shiftKey: boolean }
  ) => void;
}

function EpisodeTableSizer({
  seriesId,
  seriesType,
  columns,
  episodes,
  onMonitorEpisodePress,
}: EpisodeTableSizerProps) {
  const episodeFiles = useContext(EpisodeFileContext);
  const uiSettings = useUiSettingsValues();
  const files = new Map(episodeFiles?.map((file) => [file.id, file]));
  const visible = columns.filter((c) => c.isVisible).map((c) => c.name);
  const cells: Record<string, ReactNode[]> = {};
  const samples = new Map<Episode, string[]>();

  const addSample = (episode: Episode, name: string) =>
    samples.set(episode, [...(samples.get(episode) ?? []), name]);

  const lines: Record<string, GetText> = {
    ...LINES,
    airDateUtc: ({ airDateUtc }) =>
      getRelativeDate({ ...uiSettings, date: airDateUtc, timeForToday: true }),
  };

  const keys: Record<string, Get> = {
    ...SAMPLES,
    episodeNumber: (e) => [
      String(e.episodeNumber).length,
      seriesType === 'anime' && String(e.absoluteEpisodeNumber || '').length,
      !!getWarningMessage(
        e.unverifiedSceneNumbering,
        seriesType,
        e.absoluteEpisodeNumber
      ),
    ],
  };

  if (visible.includes('title')) {
    cells.title = Object.entries(
      Object.groupBy(episodes, (e) => e.finaleType ?? '')
    ).map(([finaleType, group = []]) => {
      group
        .filter((e) => hasBreak(e.title))
        .forEach((e) => addSample(e, 'title'));

      return (
        <VirtualTableRowCell key="title">
          <div className={rowStyles.sizerLines}>
            <EpisodeTitleLink
              episodeId={group[0].id}
              seriesId={seriesId}
              episodeEntity="episodes"
              episodeTitle={toLines(group.map((e) => e.title))}
              finaleType={finaleType || undefined}
            />
          </div>
        </VirtualTableRowCell>
      );
    });
  }

  visible.forEach((name) => {
    const getText = lines[name];
    const getKey = keys[name];

    if (getText) {
      const values = episodes.map((e) =>
        getText(e, files.get(e.episodeFileId))
      );
      const text = toLines(values);

      episodes
        .filter((_, index) => hasBreak(values[index]))
        .forEach((e) => addSample(e, name));

      if (text) {
        cells[name] = [
          <VirtualTableRowCell key={name}>
            <span className={rowStyles.sizerLines}>{text}</span>
          </VirtualTableRowCell>,
        ];
      }
    }

    if (getKey) {
      const first = new Map<string, Episode>();

      episodes.forEach((e) => {
        const value = getKey(e, files.get(e.episodeFileId));
        const key = value ? JSON.stringify(value) : '';

        if (key && !first.has(key)) {
          first.set(key, e);
        }
      });

      first.forEach((e) => addSample(e, name));
    }
  });

  const lineRows = Math.max(0, ...Object.values(cells).map((c) => c.length));

  return (
    <div className={styles.sizer} aria-hidden={true}>
      {Array.from({ length: lineRows }, (_, index) => (
        <VirtualTableRow key={index} className={rowStyles.row}>
          {visible.map((name) => cells[name]?.[index] ?? <div key={name} />)}
        </VirtualTableRow>
      ))}

      {[...samples].map(([episode, sizerColumns]) => (
        <EpisodeRow
          key={`episode-${episode.id}`}
          columns={columns}
          sizerColumns={sizerColumns}
          {...episode}
          isSaving={false}
          onMonitorEpisodePress={onMonitorEpisodePress}
        />
      ))}
    </div>
  );
}

export default memo(EpisodeTableSizer);
