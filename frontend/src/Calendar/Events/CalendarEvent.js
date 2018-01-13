import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import getStatusStyle from 'Calendar/getStatusStyle';
import episodeEntities from 'Episode/episodeEntities';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
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
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false }, () => {
      this.props.onEventModalOpenToggle(false);
    });
  }

  //
  // Render

  render() {
    const {
      id,
      series,
      title,
      seasonNumber,
      episodeNumber,
      absoluteEpisodeNumber,
      airDateUtc,
      monitored,
      hasFile,
      grabbed,
      queueItem,
      timeFormat,
      colorImpairedMode
    } = this.props;

    const startTime = moment(airDateUtc);
    const endTime = startTime.add(series.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = series.monitored && monitored;
    const statusStyle = getStatusStyle(episodeNumber, hasFile, downloading, startTime, endTime, isMonitored);
    const missingAbsoluteNumber = series.seriesType === 'anime' && seasonNumber > 0 && !absoluteEpisodeNumber;

    return (
      <div>
        <Link
          className={classNames(
            styles.event,
            styles[statusStyle],
            colorImpairedMode && 'colorImpaired'
          )}
          component="div"
          onPress={this.onPress}
        >
          <div className={styles.info}>
            <div className={styles.seriesTitle}>
              {series.title}
            </div>

            {
              missingAbsoluteNumber &&
                <Icon
                  name={icons.WARNING}
                  title="Episode does not have an absolute episode number"
                />
            }

            {
              !!queueItem &&
                <span className={styles.statusIcon}>
                  <CalendarEventQueueDetails
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
          </div>

          <div className={styles.episodeInfo}>
            <div className={styles.episodeTitle}>
              {title}
            </div>

            <div>
              {seasonNumber}x{padNumber(episodeNumber, 2)}

              {
                series.seriesType === 'anime' && absoluteEpisodeNumber &&
                  <span className={styles.absoluteEpisodeNumber}>({absoluteEpisodeNumber})</span>
              }
            </div>
          </div>

          <div>
            {formatTime(airDateUtc, timeFormat)} - {formatTime(endTime.toISOString(), timeFormat, { includeMinuteZero: true })}
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

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  series: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDateUtc: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEvent;
