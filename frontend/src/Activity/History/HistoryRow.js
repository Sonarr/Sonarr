import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import episodeEntities from 'Episode/episodeEntities';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import HistoryEventTypeCell from './HistoryEventTypeCell';
import HistoryDetailsModal from './Details/HistoryDetailsModal';
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
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      episodeId,
      series,
      episode,
      language,
      languageCutoffNotMet,
      quality,
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

            if (name === 'episodeTitle') {
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

            if (name === 'language') {
              return (
                <TableRowCell key={name}>
                  <EpisodeLanguage
                    language={language}
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
  language: PropTypes.object.isRequired,
  languageCutoffNotMet: PropTypes.bool.isRequired,
  quality: PropTypes.object.isRequired,
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
