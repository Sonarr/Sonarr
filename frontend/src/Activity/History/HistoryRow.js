import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import { icons } from 'Helpers/Props';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import formatPreferredWordScore from 'Utilities/Number/formatPreferredWordScore';
import HistoryDetailsModal from './Details/HistoryDetailsModal';
import HistoryEventTypeCell from './HistoryEventTypeCell';
import styles from './HistoryRow.css';

class HistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (
      prevProps.isMarkingAsFailed &&
      !this.props.isMarkingAsFailed &&
      !this.props.markAsFailedError
    ) {
      this.setState({ isDetailsModalOpen: false });
    }
  }

  //
  // Listeners

  onDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      episodeId,
      series,
      episode,
      languages,
      languageCutoffNotMet,
      quality,
      customFormats,
      qualityCutoffNotMet,
      eventType,
      sourceTitle,
      date,
      data,
      isMarkingAsFailed,
      columns,
      shortDateFormat,
      timeFormat,
      onMarkAsFailedPress
    } = this.props;

    if (!episode) {
      return null;
    }

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'eventType') {
              return (
                <HistoryEventTypeCell
                  key={name}
                  eventType={eventType}
                  data={data}
                />
              );
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
                <TableRowCell key={name}>
                  <SeasonEpisodeNumber
                    seasonNumber={episode.seasonNumber}
                    episodeNumber={episode.episodeNumber}
                    absoluteEpisodeNumber={episode.absoluteEpisodeNumber}
                    seriesType={series.seriesType}
                    alternateTitles={series.alternateTitles}
                    sceneSeasonNumber={episode.sceneSeasonNumber}
                    sceneEpisodeNumber={episode.sceneEpisodeNumber}
                    sceneAbsoluteEpisodeNumber={episode.sceneAbsoluteEpisodeNumber}
                  />
                </TableRowCell>
              );
            }

            if (name === 'episodes.title') {
              return (
                <TableRowCell key={name}>
                  <EpisodeTitleLink
                    episodeId={episodeId}
                    episodeEntity={episodeEntities.EPISODES}
                    seriesId={series.id}
                    episodeTitle={episode.title}
                    showOpenSeriesButton={true}
                  />
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell key={name}>
                  <EpisodeLanguages
                    languages={languages}
                    isCutoffMet={languageCutoffNotMet}
                  />
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell key={name}>
                  <EpisodeQuality
                    quality={quality}
                    isCutoffMet={qualityCutoffNotMet}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormats') {
              return (
                <TableRowCell key={name}>
                  <EpisodeFormats
                    formats={customFormats}
                  />
                </TableRowCell>
              );
            }

            if (name === 'date') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={date}
                />
              );
            }

            if (name === 'downloadClient') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.downloadClient}
                >
                  {data.downloadClient}
                </TableRowCell>
              );
            }

            if (name === 'indexer') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.indexer}
                >
                  {data.indexer}
                </TableRowCell>
              );
            }

            if (name === 'customFormatScore') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormatScore}
                >
                  {formatPreferredWordScore(data.customFormatScore)}
                </TableRowCell>
              );
            }

            if (name === 'releaseGroup') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.releaseGroup}
                >
                  {data.releaseGroup}
                </TableRowCell>
              );
            }

            if (name === 'sourceTitle') {
              return (
                <TableRowCell
                  key={name}
                >
                  {sourceTitle}
                </TableRowCell>
              );
            }

            if (name === 'details') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.details}
                >
                  <IconButton
                    name={icons.INFO}
                    onPress={this.onDetailsPress}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }

        <HistoryDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          eventType={eventType}
          sourceTitle={sourceTitle}
          data={data}
          isMarkingAsFailed={isMarkingAsFailed}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          onMarkAsFailedPress={onMarkAsFailedPress}
          onModalClose={this.onDetailsModalClose}
        />
      </TableRow>
    );
  }

}

HistoryRow.propTypes = {
  episodeId: PropTypes.number,
  series: PropTypes.object.isRequired,
  episode: PropTypes.object,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  languageCutoffNotMet: PropTypes.bool.isRequired,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  isMarkingAsFailed: PropTypes.bool,
  markAsFailedError: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default HistoryRow;
