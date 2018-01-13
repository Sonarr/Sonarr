import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeQuality from 'Episode/EpisodeQuality';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import QueueStatusCell from './QueueStatusCell';
import TimeleftCell from './TimeleftCell';
import RemoveQueueItemModal from './RemoveQueueItemModal';
import styles from './QueueRow.css';

class QueueRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isRemoveQueueItemModalOpen: false,
      isInteractiveImportModalOpen: false
    };
  }

  //
  // Listeners

  onRemoveQueueItemPress = () => {
    this.setState({ isRemoveQueueItemModalOpen: true });
  }

  onRemoveQueueItemModalConfirmed = (blacklist) => {
    this.props.onRemoveQueueItemPress(blacklist);
    this.setState({ isRemoveQueueItemModalOpen: false });
  }

  onRemoveQueueItemModalClose = () => {
    this.setState({ isRemoveQueueItemModalOpen: false });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      downloadId,
      title,
      status,
      trackedDownloadStatus,
      statusMessages,
      errorMessage,
      series,
      episode,
      quality,
      protocol,
      indexer,
      downloadClient,
      estimatedCompletionTime,
      timeleft,
      size,
      sizeleft,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      isGrabbing,
      grabError,
      isRemoving,
      isSelected,
      columns,
      onSelectedChange,
      onGrabPress
    } = this.props;

    const {
      isRemoveQueueItemModalOpen,
      isInteractiveImportModalOpen
    } = this.state;

    const progress = 100 - (sizeleft / size * 100);
    const showInteractiveImport = status === 'Completed' && trackedDownloadStatus === 'Warning';
    const isPending = status === 'Delay' || status === 'DownloadClientUnavailable';

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

            if (name === 'status') {
              return (
                <QueueStatusCell
                  key={name}
                  sourceTitle={title}
                  status={status}
                  trackedDownloadStatus={trackedDownloadStatus}
                  statusMessages={statusMessages}
                  errorMessage={errorMessage}
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

            if (name === 'series') {
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
                    unverifiedSceneNumbering={episode.unverifiedSceneNumbering}
                  />
                </TableRowCell>
              );
            }

            if (name === 'episodeTitle') {
              return (
                <TableRowCell key={name}>
                  <EpisodeTitleLink
                    episodeId={episode.id}
                    seriesId={series.id}
                    episodeFileId={episode.episodeFileId}
                    episodeTitle={episode.title}
                    showOpenSeriesButton={true}
                  />
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell key={name}>
                  <EpisodeQuality
                    quality={quality}
                  />
                </TableRowCell>
              );
            }

            if (name === 'protocol') {
              return (
                <TableRowCell key={name}>
                  <ProtocolLabel
                    protocol={protocol}
                  />
                </TableRowCell>
              );
            }

            if (name === 'indexer') {
              return (
                <TableRowCell key={name}>
                  {indexer}
                </TableRowCell>
              );
            }

            if (name === 'downloadClient') {
              return (
                <TableRowCell key={name}>
                  {downloadClient}
                </TableRowCell>
              );
            }

            if (name === 'estimatedCompletionTime') {
              return (
                <TimeleftCell
                  key={name}
                  status={status}
                  estimatedCompletionTime={estimatedCompletionTime}
                  timeleft={timeleft}
                  size={size}
                  sizeleft={sizeleft}
                  showRelativeDates={showRelativeDates}
                  shortDateFormat={shortDateFormat}
                  timeFormat={timeFormat}
                />
              );
            }

            if (name === 'progress') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.progress}
                >
                  {
                    !!progress &&
                      <ProgressBar
                        progress={progress}
                        title={`${progress.toFixed(1)}%`}
                      />
                  }
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.actions}
                >
                  {
                    showInteractiveImport &&
                      <IconButton
                        name={icons.INTERACTIVE}
                        onPress={this.onInteractiveImportPress}
                      />
                  }

                  {
                    isPending &&
                      <SpinnerIconButton
                        name={icons.DOWNLOAD}
                        kind={grabError ? kinds.DANGER : kinds.DEFAULT}
                        isSpinning={isGrabbing}
                        onPress={onGrabPress}
                      />
                  }

                  <SpinnerIconButton
                    name={icons.REMOVE}
                    isSpinning={isRemoving}
                    onPress={this.onRemoveQueueItemPress}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          downloadId={downloadId}
          title={title}
          onModalClose={this.onInteractiveImportModalClose}
        />

        <RemoveQueueItemModal
          isOpen={isRemoveQueueItemModalOpen}
          sourceTitle={title}
          onRemovePress={this.onRemoveQueueItemModalConfirmed}
          onModalClose={this.onRemoveQueueItemModalClose}
        />
      </TableRow>
    );
  }

}

QueueRow.propTypes = {
  id: PropTypes.number.isRequired,
  downloadId: PropTypes.string,
  title: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string,
  statusMessages: PropTypes.arrayOf(PropTypes.object),
  errorMessage: PropTypes.string,
  series: PropTypes.object.isRequired,
  episode: PropTypes.object.isRequired,
  quality: PropTypes.object.isRequired,
  protocol: PropTypes.string.isRequired,
  indexer: PropTypes.string,
  downloadClient: PropTypes.string,
  estimatedCompletionTime: PropTypes.string,
  timeleft: PropTypes.string,
  size: PropTypes.number,
  sizeleft: PropTypes.number,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isGrabbing: PropTypes.bool.isRequired,
  grabError: PropTypes.object,
  isRemoving: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onGrabPress: PropTypes.func.isRequired,
  onRemoveQueueItemPress: PropTypes.func.isRequired
};

QueueRow.defaultProps = {
  isGrabbing: false,
  isRemoving: false
};

export default QueueRow;
