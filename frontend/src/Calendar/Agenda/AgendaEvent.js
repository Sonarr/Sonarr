import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import episodeEntities from 'Episode/episodeEntities';
import { icons, kinds } from 'Helpers/Props';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import styles from './AgendaEvent.css';

class AgendaEvent extends Component {
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
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
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
      showDate,
      showEpisodeInformation,
      showFinaleIcon,
      showSpecialIcon,
      showCutoffUnmetIcon,
      timeFormat,
      longDateFormat,
      colorImpairedMode
    } = this.props;

    const startTime = moment(airDateUtc);
    const endTime = moment(airDateUtc).add(series.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = series.monitored && monitored;
    const statusStyle = getStatusStyle(hasFile, downloading, startTime, endTime, isMonitored);
    const missingAbsoluteNumber = series.seriesType === 'anime' && seasonNumber > 0 && !absoluteEpisodeNumber;
    const season = series.seasons.find((s) => s.seasonNumber === seasonNumber);
    const seasonStatistics = season?.statistics || {};

    return (
      <div>
        <Link
          className={styles.event}
          component="div"
          onPress={this.onPress}
        >
          <div className={styles.date}>
            {
              showDate &&
                startTime.format(longDateFormat)
            }
          </div>

          <div
            className={classNames(
              styles.eventWrapper,
              styles[statusStyle],
              colorImpairedMode && 'colorImpaired'
            )}
          >
            <div className={styles.time}>
              {formatTime(airDateUtc, timeFormat)} - {formatTime(endTime.toISOString(), timeFormat, { includeMinuteZero: true })}
            </div>

            <div className={styles.seriesTitle}>
              {series.title}
            </div>

            {
              showEpisodeInformation &&
                <div className={styles.seasonEpisodeNumber}>
                  {seasonNumber}x{padNumber(episodeNumber, 2)}

                  {
                    series.seriesType === 'anime' && absoluteEpisodeNumber &&
                      <span className={styles.absoluteEpisodeNumber}>({absoluteEpisodeNumber})</span>
                  }

                  <div className={styles.episodeSeparator}> - </div>
                </div>
            }

            <div className={styles.episodeTitle}>
              {
                showEpisodeInformation &&
                title
              }
            </div>

            {
              missingAbsoluteNumber &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.WARNING}
                  title="Episode does not have an absolute episode number"
                />
            }

            {
              unverifiedSceneNumbering && !missingAbsoluteNumber ?
                <Icon
                  className={styles.statusIcon}
                  name={icons.WARNING}
                  title="Scene number hasn't been verified yet"
                /> :
                null
            }

            {
              !!queueItem &&
                <span className={styles.statusIcon}>
                  <CalendarEventQueueDetails
                    seriesType={series.seriesType}
                    seasonNumber={seasonNumber}
                    absoluteEpisodeNumber={absoluteEpisodeNumber}
                    {...queueItem}
                  />
                </span>
            }

            {
              !queueItem && grabbed &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.DOWNLOADING}
                  title="Episode is downloading"
                />
            }

            {
              showCutoffUnmetIcon &&
              !!episodeFile &&
              episodeFile.qualityCutoffNotMet &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.EPISODE_FILE}
                  kind={kinds.WARNING}
                  title="Quality cutoff has not been met"
                />
            }

            {
              showCutoffUnmetIcon &&
              !!episodeFile &&
              episodeFile.languageCutoffNotMet &&
              !episodeFile.qualityCutoffNotMet &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.EPISODE_FILE}
                  kind={kinds.WARNING}
                  title="Language cutoff has not been met"
                />
            }

            {
              episodeNumber === 1 && seasonNumber > 0 &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.INFO}
                  kind={kinds.INFO}
                  title={seasonNumber === 1 ? 'Series Premiere' : 'Season Premiere'}
                />
            }

            {
              showFinaleIcon &&
              episodeNumber !== 1 &&
              seasonNumber > 0 &&
              episodeNumber === seasonStatistics.totalEpisodeCount &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.INFO}
                  kind={kinds.WARNING}
                  title={series.status === 'ended' ? 'Series finale' : 'Season finale'}
                />
            }

            {
              showSpecialIcon &&
              (episodeNumber === 0 || seasonNumber === 0) &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.INFO}
                  kind={kinds.PINK}
                  title="Special"
                />
            }
          </div>
        </Link>

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

AgendaEvent.propTypes = {
  id: PropTypes.number.isRequired,
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
  showDate: PropTypes.bool.isRequired,
  showEpisodeInformation: PropTypes.bool.isRequired,
  showFinaleIcon: PropTypes.bool.isRequired,
  showSpecialIcon: PropTypes.bool.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default AgendaEvent;
