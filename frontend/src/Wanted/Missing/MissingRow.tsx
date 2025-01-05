import React from 'react';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import EpisodeSearchCell from 'Episode/EpisodeSearchCell';
import EpisodeStatus from 'Episode/EpisodeStatus';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import useSeries from 'Series/useSeries';
import { SelectStateInputProps } from 'typings/props';
import styles from './MissingRow.css';

interface MissingRowProps {
  id: number;
  seriesId: number;
  episodeFileId?: number;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  unverifiedSceneNumbering: boolean;
  airDateUtc?: string;
  lastSearchTime?: string;
  title: string;
  isSelected?: boolean;
  columns: Column[];
  onSelectedChange: (options: SelectStateInputProps) => void;
}

function MissingRow({
  id,
  seriesId,
  episodeFileId = 0,
  seasonNumber,
  episodeNumber,
  absoluteEpisodeNumber,
  sceneSeasonNumber,
  sceneEpisodeNumber,
  sceneAbsoluteEpisodeNumber,
  unverifiedSceneNumbering,
  airDateUtc,
  lastSearchTime,
  title,
  isSelected,
  columns,
  onSelectedChange,
}: MissingRowProps) {
  const series = useSeries(seriesId);

  if (!series) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'series.sortTitle') {
          return (
            <TableRowCell key={name}>
              <SeriesTitleLink
                titleSlug={series.titleSlug}
                title={series.title}
              />
            </TableRowCell>
          );
        }

        if (name === 'episode') {
          return (
            <TableRowCell key={name} className={styles.episode}>
              <SeasonEpisodeNumber
                seasonNumber={seasonNumber}
                episodeNumber={episodeNumber}
                absoluteEpisodeNumber={absoluteEpisodeNumber}
                seriesType={series.seriesType}
                alternateTitles={series.alternateTitles}
                sceneSeasonNumber={sceneSeasonNumber}
                sceneEpisodeNumber={sceneEpisodeNumber}
                sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
                unverifiedSceneNumbering={unverifiedSceneNumbering}
              />
            </TableRowCell>
          );
        }

        if (name === 'episodes.title') {
          return (
            <TableRowCell key={name}>
              <EpisodeTitleLink
                episodeId={id}
                seriesId={series.id}
                episodeEntity="wanted.missing"
                episodeTitle={title}
                showOpenSeriesButton={true}
              />
            </TableRowCell>
          );
        }

        if (name === 'episodes.airDateUtc') {
          return <RelativeDateCell key={name} date={airDateUtc} />;
        }

        if (name === 'episodes.lastSearchTime') {
          return (
            <RelativeDateCell
              key={name}
              date={lastSearchTime}
              includeSeconds={true}
            />
          );
        }

        if (name === 'status') {
          return (
            <TableRowCell key={name} className={styles.status}>
              <EpisodeStatus
                episodeId={id}
                episodeFileId={episodeFileId}
                episodeEntity="wanted.missing"
              />
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <EpisodeSearchCell
              key={name}
              episodeId={id}
              seriesId={series.id}
              episodeTitle={title}
              episodeEntity="wanted.missing"
              showOpenSeriesButton={true}
            />
          );
        }

        return null;
      })}
    </TableRow>
  );
}

export default MissingRow;
