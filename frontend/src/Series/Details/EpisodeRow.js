import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeNumber from 'Episode/EpisodeNumber';
import EpisodeSearchCellConnector from 'Episode/EpisodeSearchCellConnector';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeFileLanguageConnector from 'EpisodeFile/EpisodeFileLanguageConnector';
import MediaInfoConnector from 'EpisodeFile/MediaInfoConnector';
import * as mediaInfoTypes from 'EpisodeFile/mediaInfoTypes';
import formatBytes from 'Utilities/Number/formatBytes';
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
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  onMonitorEpisodePress = (monitored, options) => {
    this.props.onMonitorEpisodePress(this.props.id, monitored, options);
  };

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
      useSceneNumbering,
      unverifiedSceneNumbering,
      isSaving,
      seriesMonitored,
      seriesType,
      episodeFilePath,
      episodeFileRelativePath,
      episodeFileSize,
      releaseGroup,
      customFormats,
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
                  className={seriesType === 'anime' ? styles.episodeNumberAnime : styles.episodeNumber}
                >
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

            if (name === 'customFormats') {
              return (
                <TableRowCell key={name}>
                  <EpisodeFormats
                    formats={customFormats}
                  />
                </TableRowCell>
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

            if (name === 'audioLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audioLanguages}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO_LANGUAGES}
                    episodeFileId={episodeFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'subtitleLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.subtitles}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.SUBTITLES}
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

            if (name === 'videoDynamicRangeType') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.videoDynamicRangeType}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO_DYNAMIC_RANGE_TYPE}
                    episodeFileId={episodeFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'size') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.size}
                >
                  {!!episodeFileSize && formatBytes(episodeFileSize)}
                </TableRowCell>
              );
            }

            if (name === 'releaseGroup') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.releaseGroup}
                >
                  {releaseGroup}
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
  useSceneNumbering: PropTypes.bool,
  unverifiedSceneNumbering: PropTypes.bool,
  seriesMonitored: PropTypes.bool.isRequired,
  seriesType: PropTypes.string.isRequired,
  episodeFilePath: PropTypes.string,
  episodeFileRelativePath: PropTypes.string,
  episodeFileSize: PropTypes.number,
  releaseGroup: PropTypes.string,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  mediaInfo: PropTypes.object,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired
};

EpisodeRow.defaultProps = {
  alternateTitles: [],
  customFormats: []
};

export default EpisodeRow;
