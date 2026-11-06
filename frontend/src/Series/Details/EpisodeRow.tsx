import React, { memo, useCallback } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell, {
  VirtualTableRowCellProps,
} from 'Components/Table/Cells/VirtualTableRowCell';
import virtualCellStyles from 'Components/Table/Cells/VirtualTableRowCell.module.css';
import Column from 'Components/Table/Column';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeNumber from 'Episode/EpisodeNumber';
import EpisodeSearchCell from 'Episode/EpisodeSearchCell';
import EpisodeStatus from 'Episode/EpisodeStatus';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import IndexerFlags from 'Episode/IndexerFlags';
import EpisodeFileLanguages from 'EpisodeFile/EpisodeFileLanguages';
import { useEpisodeFile } from 'EpisodeFile/EpisodeFileProvider';
import MediaInfo from 'EpisodeFile/MediaInfo';
import { icons } from 'Helpers/Props';
import { useSingleSeries } from 'Series/useSeries';
import MediaInfoModel from 'typings/MediaInfo';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import formatRuntime from 'Utilities/Number/formatRuntime';
import translate from 'Utilities/String/translate';
import styles from './EpisodeRow.module.css';

interface EpisodeRowProps {
  id: number;
  seriesId: number;
  episodeFileId?: number;
  monitored: boolean;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  airDateUtc?: string;
  runtime?: number;
  finaleType?: string;
  title: string;
  isSaving?: boolean;
  unverifiedSceneNumbering?: boolean;
  // episodeFilePath?: string;
  // episodeFileRelativePath?: string;
  // episodeFileSize?: number;
  // releaseGroup?: string;
  // customFormats?: CustomFormat[];
  // customFormatScore: number;
  // indexerFlags?: number;
  mediaInfo?: MediaInfoModel;
  columns: Column[];
  sizerColumns?: string[];
  onMonitorEpisodePress: (
    episodeId: number,
    value: boolean,
    { shiftKey }: { shiftKey: boolean }
  ) => void;
}

function RowCell(props: VirtualTableRowCellProps) {
  return <VirtualTableRowCell role="cell" {...props} />;
}

function EpisodeRow({
  id,
  seriesId,
  episodeFileId,
  monitored,
  seasonNumber,
  episodeNumber,
  absoluteEpisodeNumber,
  sceneSeasonNumber,
  sceneEpisodeNumber,
  sceneAbsoluteEpisodeNumber,
  airDateUtc,
  runtime,
  finaleType,
  title,
  unverifiedSceneNumbering,
  isSaving,
  // episodeFilePath,
  // episodeFileRelativePath,
  // episodeFileSize,
  // releaseGroup,
  // customFormats = [],
  // customFormatScore,
  // indexerFlags = 0,
  columns,
  sizerColumns,
  onMonitorEpisodePress,
}: EpisodeRowProps) {
  const {
    title: seriesTitle,
    useSceneNumbering,
    monitored: seriesMonitored,
    seriesType,
    alternateTitles = [],
  } = useSingleSeries(seriesId)!;
  const episodeFile = useEpisodeFile(episodeFileId);

  const customFormats = episodeFile?.customFormats ?? [];
  const customFormatScore = episodeFile?.customFormatScore ?? 0;

  const handleMonitorEpisodePress = useCallback(
    (monitored: boolean, options: { shiftKey: boolean }) => {
      onMonitorEpisodePress(id, monitored, options);
    },
    [id, onMonitorEpisodePress]
  );

  return (
    <VirtualTableRow className={styles.row} role="row">
      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (sizerColumns && !sizerColumns.includes(name)) {
          return <div key={name} />;
        }

        if (name === 'monitored') {
          return (
            <RowCell key={name}>
              <MonitorToggleButton
                monitored={monitored}
                isDisabled={!seriesMonitored && !monitored}
                isSaving={isSaving}
                onPress={handleMonitorEpisodePress}
              />
            </RowCell>
          );
        }

        if (name === 'episodeNumber') {
          return (
            <RowCell key={name}>
              <EpisodeNumber
                seasonNumber={seasonNumber}
                episodeNumber={episodeNumber}
                absoluteEpisodeNumber={absoluteEpisodeNumber}
                useSceneNumbering={useSceneNumbering}
                unverifiedSceneNumbering={unverifiedSceneNumbering}
                seriesType={seriesType}
                sceneSeasonNumber={sceneSeasonNumber}
                sceneEpisodeNumber={sceneEpisodeNumber}
                sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
                alternateTitles={alternateTitles}
              />
            </RowCell>
          );
        }

        if (name === 'title') {
          return (
            <RowCell key={name}>
              <EpisodeTitleLink
                episodeId={id}
                seriesId={seriesId}
                episodeTitle={title}
                episodeEntity="episodes"
                finaleType={finaleType}
              />
            </RowCell>
          );
        }

        if (name === 'path') {
          return <RowCell key={name}>{episodeFile?.path}</RowCell>;
        }

        if (name === 'relativePath') {
          return <RowCell key={name}>{episodeFile?.relativePath}</RowCell>;
        }

        if (name === 'airDateUtc') {
          return (
            <RelativeDateCell
              key={name}
              className={virtualCellStyles.cell}
              date={airDateUtc}
              component={RowCell}
            />
          );
        }

        if (name === 'runtime') {
          return <RowCell key={name}>{formatRuntime(runtime)}</RowCell>;
        }

        if (name === 'customFormats') {
          return (
            <RowCell key={name}>
              <EpisodeFormats formats={customFormats} />
            </RowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <RowCell key={name}>
              <Tooltip
                anchor={formatCustomFormatScore(
                  customFormatScore,
                  customFormats.length
                )}
                tooltip={<EpisodeFormats formats={customFormats} />}
                position="left"
              />
            </RowCell>
          );
        }

        if (name === 'languages') {
          return (
            <RowCell key={name}>
              <EpisodeFileLanguages episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'audioInfo') {
          return (
            <RowCell key={name}>
              <MediaInfo type="audio" episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'audioLanguages') {
          return (
            <RowCell key={name}>
              <MediaInfo type="audioLanguages" episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'subtitleLanguages') {
          return (
            <RowCell key={name}>
              <MediaInfo type="subtitles" episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'videoCodec') {
          return (
            <RowCell key={name}>
              <MediaInfo type="video" episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'videoDynamicRangeType') {
          return (
            <RowCell key={name}>
              <MediaInfo
                type="videoDynamicRangeType"
                episodeFileId={episodeFileId}
              />
            </RowCell>
          );
        }

        if (name === 'size') {
          return (
            <RowCell key={name}>
              {!!episodeFile?.size && formatBytes(episodeFile?.size)}
            </RowCell>
          );
        }

        if (name === 'releaseGroup') {
          return <RowCell key={name}>{episodeFile?.releaseGroup}</RowCell>;
        }

        if (name === 'indexerFlags') {
          return (
            <RowCell key={name}>
              {episodeFile?.indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} kind="default" />}
                  title={translate('IndexerFlags')}
                  body={
                    <IndexerFlags indexerFlags={episodeFile?.indexerFlags} />
                  }
                  position="left"
                />
              ) : null}
            </RowCell>
          );
        }

        if (name === 'status') {
          return (
            <RowCell key={name} className={styles.status}>
              <EpisodeStatus episodeId={id} episodeFileId={episodeFileId} />
            </RowCell>
          );
        }

        if (name === 'actions') {
          return (
            <EpisodeSearchCell
              key={name}
              className={virtualCellStyles.cell}
              component={RowCell}
              episodeId={id}
              episodeEntity="episodes"
              seriesId={seriesId}
              seriesTitle={seriesTitle}
              seasonNumber={seasonNumber}
              episodeNumber={episodeNumber}
              episodeTitle={title}
            />
          );
        }

        return null;
      })}
    </VirtualTableRow>
  );
}

export default memo(EpisodeRow);
