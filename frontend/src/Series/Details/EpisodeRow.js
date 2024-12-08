import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeNumber from 'Episode/EpisodeNumber';
import EpisodeSearchCell from 'Episode/EpisodeSearchCell';
import EpisodeStatus from 'Episode/EpisodeStatus';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import IndexerFlags from 'Episode/IndexerFlags';
import EpisodeFileLanguages from 'EpisodeFile/EpisodeFileLanguages';
import MediaInfo from 'EpisodeFile/MediaInfo';
import * as mediaInfoTypes from 'EpisodeFile/mediaInfoTypes';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import formatRuntime from 'Utilities/Number/formatRuntime';
import translate from 'Utilities/String/translate';
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
      runtime,
      finaleType,
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
      customFormatScore,
      indexerFlags,
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
                    episodeEntity="episodes"
                    finaleType={finaleType}
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
                <RelativeDateCell
                  key={name}
                  date={airDateUtc}
                />
              );
            }

            if (name === 'runtime') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.runtime}
                >
                  { formatRuntime(runtime) }
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

            if (name === 'customFormatScore') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormatScore}
                >
                  <Tooltip
                    anchor={formatCustomFormatScore(
                      customFormatScore,
                      customFormats.length
                    )}
                    tooltip={<EpisodeFormats formats={customFormats} />}
                    position={tooltipPositions.LEFT}
                  />
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.languages}
                >
                  <EpisodeFileLanguages
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
                  <MediaInfo
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
                  <MediaInfo
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
                  <MediaInfo
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
                  <MediaInfo
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
                  <MediaInfo
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

            if (name === 'indexerFlags') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.indexerFlags}
                >
                  {indexerFlags ? (
                    <Popover
                      anchor={<Icon name={icons.FLAG} kind={kinds.PRIMARY} />}
                      title={translate('IndexerFlags')}
                      body={<IndexerFlags indexerFlags={indexerFlags} />}
                      position={tooltipPositions.LEFT}
                    />
                  ) : null}
                </TableRowCell>
              );
            }

            if (name === 'status') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.status}
                >
                  <EpisodeStatus
                    episodeId={id}
                    episodeFileId={episodeFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <EpisodeSearchCell
                  key={name}
                  episodeId={id}
                  episodeEntity='episodes'
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
  runtime: PropTypes.number,
  finaleType: PropTypes.string,
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
  customFormatScore: PropTypes.number.isRequired,
  indexerFlags: PropTypes.number.isRequired,
  mediaInfo: PropTypes.object,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired
};

EpisodeRow.defaultProps = {
  alternateTitles: [],
  customFormats: [],
  indexerFlags: 0
};

export default EpisodeRow;
