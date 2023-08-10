import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import episodeEntities from 'Episode/episodeEntities';
import { icons, kinds } from 'Helpers/Props';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

class CalendarEvent extends Component {

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

  onPress = () => {
    this.setState({ isDetailsModalOpen: true }, () => {
      this.props.onEventModalOpenToggle(true);
    });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false }, () => {
      this.props.onEventModalOpenToggle(false);
    });
  };

  //
  // Render

  render() {
    const {
      id,
      series,
      episodeFile,
      title,
      seasonNumber,
      episodeNumber,
      absoluteEpisodeNumber,
      airDateUtc,
      monitored,
      unverifiedSceneNumbering,
      hasFile,
      grabbed,
      queueItem,
      showEpisodeInformation,
      showFinaleIcon,
      showSpecialIcon,
      showCutoffUnmetIcon,
      fullColorEvents,
      timeFormat,
      colorImpairedMode
    } = this.props;

    if (!series) {
      return null;
    }

    const startTime = moment(airDateUtc);
    const endTime = moment(airDateUtc).add(series.runtime, 'minutes');
    const isDownloading = !!(queueItem || grabbed);
    const isMonitored = series.monitored && monitored;
    const statusStyle = getStatusStyle(hasFile, isDownloading, startTime, endTime, isMonitored);
    const missingAbsoluteNumber = series.seriesType === 'anime' && seasonNumber > 0 && !absoluteEpisodeNumber;
    const season = series.seasons.find((s) => s.seasonNumber === seasonNumber);
    const seasonStatistics = season?.statistics || {};

    return (
      <div
        className={classNames(
          styles.event,
          styles[statusStyle],
          colorImpairedMode && 'colorImpaired',
          fullColorEvents && 'fullColor'
        )}
      >
        <Link
          className={styles.underlay}
          onPress={this.onPress}
        />

        <div className={styles.overlay} >
          <div className={styles.info}>
            <div className={styles.seriesTitle}>
              {series.title}
            </div>

            <div className={styles.statusContainer}>
              {
                missingAbsoluteNumber ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.WARNING}
                    title={translate('EpisodeMissingAbsoluteNumber')}
                  /> :
                  null
              }

              {
                unverifiedSceneNumbering && !missingAbsoluteNumber ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.WARNING}
                    title={translate('SceneNumberNotVerified')}
                  /> :
                  null
              }

              {
                queueItem ?
                  <span className={styles.statusIcon}>
                    <CalendarEventQueueDetails
                      {...queueItem}
                    />
                  </span> :
                  null
              }

              {
                !queueItem && grabbed ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.DOWNLOADING}
                    title={translate('EpisodeIsDownloading')}
                  /> :
                  null
              }

              {
                showCutoffUnmetIcon &&
                !!episodeFile &&
                episodeFile.qualityCutoffNotMet ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.EPISODE_FILE}
                    kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
                    title={translate('QualityCutoffNotMet')}
                  /> :
                  null
              }

              {
                episodeNumber === 1 && seasonNumber > 0 ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.INFO}
                    kind={kinds.INFO}
                    darken={fullColorEvents}
                    title={seasonNumber === 1 ? translate('SeriesPremiere') : translate('SeasonPremiere')}
                  /> :
                  null
              }

              {
                showFinaleIcon &&
                episodeNumber !== 1 &&
                seasonNumber > 0 &&
                episodeNumber === seasonStatistics.totalEpisodeCount ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.INFO}
                    kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
                    title={series.status === 'ended' ? translate('SeriesFinale') : translate('SeasonFinale')}
                  /> :
                  null
              }

              {
                showSpecialIcon &&
                (episodeNumber === 0 || seasonNumber === 0) ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.INFO}
                    kind={kinds.PINK}
                    darken={fullColorEvents}
                    title={translate('Special')}
                  /> :
                  null
              }
            </div>
          </div>

          {
            showEpisodeInformation ?
              <div className={styles.episodeInfo}>
                <div className={styles.episodeTitle}>
                  {title}
                </div>

                <div>
                  {seasonNumber}x{padNumber(episodeNumber, 2)}

                  {
                    series.seriesType === 'anime' && absoluteEpisodeNumber ?
                      <span className={styles.absoluteEpisodeNumber}>({absoluteEpisodeNumber})</span> : null
                  }
                </div>
              </div> :
              null
          }

          <div className={styles.airTime}>
            {formatTime(airDateUtc, timeFormat)} - {formatTime(endTime.toISOString(), timeFormat, { includeMinuteZero: true })}
          </div>
        </div>

        <EpisodeDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          episodeId={id}
          episodeEntity={episodeEntities.CALENDAR}
          seriesId={series.id}
          episodeTitle={title}
          showOpenSeriesButton={true}
          onModalClose={this.onDetailsModalClose}
        />
      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  episodeId: PropTypes.number.isRequired,
  series: PropTypes.object.isRequired,
  episodeFile: PropTypes.object,
  title: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDateUtc: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  unverifiedSceneNumbering: PropTypes.bool,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  // These props come from the connector, not marked as required to appease TS for now.
  showEpisodeInformation: PropTypes.bool,
  showFinaleIcon: PropTypes.bool,
  showSpecialIcon: PropTypes.bool,
  showCutoffUnmetIcon: PropTypes.bool,
  fullColorEvents: PropTypes.bool,
  timeFormat: PropTypes.string,
  colorImpairedMode: PropTypes.bool,
  onEventModalOpenToggle: PropTypes.func
};

export default CalendarEvent;
