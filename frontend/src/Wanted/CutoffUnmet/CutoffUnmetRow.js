import PropTypes from 'prop-types';
import React from 'react';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import EpisodeSearchCellConnector from 'Episode/EpisodeSearchCellConnector';
import EpisodeFileLanguageConnector from 'EpisodeFile/EpisodeFileLanguageConnector';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import styles from './CutoffUnmetRow.css';

function CutoffUnmetRow(props) {
  const {
    id,
    episodeFileId,
    series,
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    unverifiedSceneNumbering,
    airDateUtc,
    title,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {
        columns.map((column) => {
          const {
            name,
            isVisible
          } = column;

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
              <TableRowCell
                key={name}
                className={styles.episode}
              >
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

          if (name === 'episodeTitle') {
            return (
              <TableRowCell key={name}>
                <EpisodeTitleLink
                  episodeId={id}
                  seriesId={series.id}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                  episodeTitle={title}
                  showOpenSeriesButton={true}
                />
              </TableRowCell>
            );
          }

          if (name === 'airDateUtc') {
            return (
              <RelativeDateCellConnector
                key={name}
                date={airDateUtc}
              />
            );
          }

          if (name === 'language') {
            return (
              <TableRowCell
                key={name}
                className={styles.language}
              >
                <EpisodeFileLanguageConnector
                  episodeFileId={episodeFileId}
                />
              </TableRowCell>
            );
          }

          if (name === 'status') {
            return (
              <TableRowCell
                key={name}
                className={styles.status}
              >
                <EpisodeStatusConnector
                  episodeId={id}
                  episodeFileId={episodeFileId}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <EpisodeSearchCellConnector
                key={name}
                episodeId={id}
                seriesId={series.id}
                episodeTitle={title}
                episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                showOpenSeriesButton={true}
              />
            );
          }

          return null;
        })
      }
    </TableRow>
  );
}

CutoffUnmetRow.propTypes = {
  id: PropTypes.number.isRequired,
  episodeFileId: PropTypes.number,
  series: PropTypes.object.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  unverifiedSceneNumbering: PropTypes.bool.isRequired,
  airDateUtc: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default CutoffUnmetRow;
