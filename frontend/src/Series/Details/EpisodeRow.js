import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import EpisodeSearchCellConnector from 'Episode/EpisodeSearchCellConnector';
import EpisodeNumber from 'Episode/EpisodeNumber';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';
import EpisodeFileLanguageConnector from 'EpisodeFile/EpisodeFileLanguageConnector';
import MediaInfoConnector from 'EpisodeFile/MediaInfoConnector';
import * as mediaInfoTypes from 'EpisodeFile/mediaInfoTypes';

import styles from './EpisodeRow.css';

class EpisodeRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  onMonitorEpisodePress = (monitored, options) => {
    this.props.onMonitorEpisodePress(this.props.id, monitored, options);
  }

  //
  // Render

  render() {
    const {
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
      title,
      unverifiedSceneNumbering,
      isSaving,
      seriesMonitored,
      seriesType,
      episodeFilePath,
      episodeFileRelativePath,
      alternateTitles,
      columns
    } = this.props;

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

            if (name === 'monitored') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.monitored}
                >
                  <MonitorToggleButton
                    monitored={monitored}
                    isDisabled={!seriesMonitored}
                    isSaving={isSaving}
                    onPress={this.onMonitorEpisodePress}
                  />
                </TableRowCell>
              );
            }

            if (name === 'episodeNumber') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.episodeNumber}
                >
                  <EpisodeNumber
                    seasonNumber={seasonNumber}
                    episodeNumber={episodeNumber}
                    absoluteEpisodeNumber={absoluteEpisodeNumber}
                    unverifiedSceneNumbering={unverifiedSceneNumbering}
                    seriesType={seriesType}
                    sceneSeasonNumber={sceneSeasonNumber}
                    sceneEpisodeNumber={sceneEpisodeNumber}
                    sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
                    alternateTitles={alternateTitles}
                  />
                </TableRowCell>
              );
            }

            if (name === 'title') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  <EpisodeTitleLink
                    episodeId={id}
                    seriesId={seriesId}
                    episodeTitle={title}
                    showOpenSeriesButton={false}
                  />
                </TableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <TableRowCell key={name}>
                  {
                    episodeFilePath
                  }
                </TableRowCell>
              );
            }

            if (name === 'relativePath') {
              return (
                <TableRowCell key={name}>
                  {
                    episodeFileRelativePath
                  }
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

            if (name === 'audioInfo') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audio}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO}
                    episodeFileId={episodeFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'videoCodec') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.video}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO}
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
                  />
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <EpisodeSearchCellConnector
                  key={name}
                  episodeId={id}
                  seriesId={seriesId}
                  episodeTitle={title}
                />
              );
            }

            return null;
          })
        }
      </TableRow>
    );
  }
}

EpisodeRow.propTypes = {
  id: PropTypes.number.isRequired,
  seriesId: PropTypes.number.isRequired,
  episodeFileId: PropTypes.number,
  monitored: PropTypes.bool.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  airDateUtc: PropTypes.string,
  title: PropTypes.string.isRequired,
  isSaving: PropTypes.bool,
  unverifiedSceneNumbering: PropTypes.bool,
  seriesMonitored: PropTypes.bool.isRequired,
  seriesType: PropTypes.string.isRequired,
  episodeFilePath: PropTypes.string,
  episodeFileRelativePath: PropTypes.string,
  mediaInfo: PropTypes.object,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired
};

export default EpisodeRow;
