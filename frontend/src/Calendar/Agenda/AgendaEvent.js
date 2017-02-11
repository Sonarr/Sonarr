import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import { icons } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
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
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
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
      showDate,
      timeFormat,
      longDateFormat
    } = this.props;

    const startTime = moment(airDateUtc);
    const endTime = startTime.add(series.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = series.monitored && monitored;
    const statusStyle = getStatusStyle(episodeNumber, hasFile, downloading, startTime, endTime, isMonitored);

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
              styles.status,
              styles[statusStyle]
            )}
          />

          <div className={styles.time}>
            {formatTime(airDateUtc, timeFormat)} - {formatTime(endTime.toISOString(), timeFormat, { includeMinuteZero: true })}
          </div>

          <div className={styles.seriesTitle}>
            {series.title}
          </div>

          <div className={styles.seasonEpisodeNumber}>
            {seasonNumber}x{padNumber(episodeNumber, 2)}

            {
              series.seriesType === 'anime' && absoluteEpisodeNumber &&
                <span className={styles.absoluteEpisodeNumber}>({absoluteEpisodeNumber})</span>
            }

            <div className={styles.episodeSeparator}> - </div>
          </div>

          <div className={styles.episodeTitle}>
            {title}
          </div>

          {
            !!queueItem &&
              <CalendarEventQueueDetails
                seriesType={series.seriesType}
                seasonNumber={seasonNumber}
                absoluteEpisodeNumber={absoluteEpisodeNumber}
                {...queueItem}
              />
          }

          {
            !queueItem && grabbed &&
              <Icon
                name={icons.DOWNLOADING}
                title="Episode is downloading"
              />
          }
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
  title: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDateUtc: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showDate: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

export default AgendaEvent;
