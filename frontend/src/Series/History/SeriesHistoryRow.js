import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Popover from 'Components/Tooltip/Popover';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import EpisodeNumber from 'Episode/EpisodeNumber';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import HistoryDetailsConnector from 'Activity/History/Details/HistoryDetailsConnector';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import styles from './SeriesHistoryRow.css';

function getTitle(eventType) {
  switch (eventType) {
    case 'grabbed': return 'Grabbed';
    case 'seriesFolderImported': return 'Series Folder Imported';
    case 'downloadFolderImported': return 'Download Folder Imported';
    case 'downloadFailed': return 'Download Failed';
    case 'episodeFileDeleted': return 'Episode File Deleted';
    case 'episodeFileRenamed': return 'Episode File Renamed';
    default: return 'Unknown';
  }
}

class SeriesHistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMarkAsFailedModalOpen: false
    };
  }

  //
  // Listeners

  onMarkAsFailedPress = () => {
    this.setState({ isMarkAsFailedModalOpen: true });
  }

  onConfirmMarkAsFailed = () => {
    this.props.onMarkAsFailedPress(this.props.id);
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  onMarkAsFailedModalClose = () => {
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      eventType,
      sourceTitle,
      language,
      languageCutoffNotMet,
      quality,
      qualityCutoffNotMet,
      date,
      data,
      fullSeries,
      series,
      episode
    } = this.props;

    const {
      isMarkAsFailedModalOpen
    } = this.state;

    const EpisodeComponent = fullSeries ? SeasonEpisodeNumber : EpisodeNumber;

    return (
      <TableRow>
        <HistoryEventTypeCell
          eventType={eventType}
          data={data}
        />

        <TableRowCell key={name}>
          <EpisodeComponent
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

        <TableRowCell>
          {sourceTitle}
        </TableRowCell>

        <TableRowCell>
          <EpisodeLanguage
            language={language}
            isCutoffNotMet={languageCutoffNotMet}
          />
        </TableRowCell>

        <TableRowCell>
          <EpisodeQuality
            quality={quality}
            isCutoffNotMet={qualityCutoffNotMet}
          />
        </TableRowCell>

        <RelativeDateCellConnector
          date={date}
        />

        <TableRowCell className={styles.details}>
          <Popover
            anchor={
              <Icon
                name={icons.INFO}
              />
            }
            title={getTitle(eventType)}
            body={
              <HistoryDetailsConnector
                eventType={eventType}
                sourceTitle={sourceTitle}
                data={data}
              />
            }
            position={tooltipPositions.LEFT}
          />
        </TableRowCell>

        <TableRowCell className={styles.actions}>
          {
            eventType === 'grabbed' &&
              <IconButton
                name={icons.REMOVE}
                onPress={this.onMarkAsFailedPress}
              />
          }
        </TableRowCell>

        <ConfirmModal
          isOpen={isMarkAsFailedModalOpen}
          kind={kinds.DANGER}
          title="Mark as Failed"
          message={`Are you sure you want to mark '${sourceTitle}' as failed?`}
          confirmLabel="Mark as Failed"
          onConfirm={this.onConfirmMarkAsFailed}
          onCancel={this.onMarkAsFailedModalClose}
        />
      </TableRow>
    );
  }
}

SeriesHistoryRow.propTypes = {
  id: PropTypes.number.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired,
  languageCutoffNotMet: PropTypes.bool.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  fullSeries: PropTypes.bool.isRequired,
  series: PropTypes.object.isRequired,
  episode: PropTypes.object.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default SeriesHistoryRow;
