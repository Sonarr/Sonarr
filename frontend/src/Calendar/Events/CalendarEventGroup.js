import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CalendarEventConnector from 'Calendar/Events/CalendarEventConnector';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import getFinaleTypeName from 'Episode/getFinaleTypeName';
import { icons, kinds } from 'Helpers/Props';
import formatTime from 'Utilities/Date/formatTime';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import styles from './CalendarEventGroup.css';

function getEventsInfo(series, events) {
  let files = 0;
  let queued = 0;
  let monitored = 0;
  let absoluteEpisodeNumbers = 0;

  events.forEach((event) => {
    if (event.episodeFileId) {
      files++;
    }

    if (event.queued) {
      queued++;
    }

    if (series.monitored && event.monitored) {
      monitored++;
    }

    if (event.absoluteEpisodeNumber) {
      absoluteEpisodeNumbers++;
    }
  });

  return {
    allDownloaded: files === events.length,
    anyQueued: queued > 0,
    anyMonitored: monitored > 0,
    allAbsoluteEpisodeNumbers: absoluteEpisodeNumbers === events.length
  };
}

class CalendarEventGroup extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isExpanded: false
    };
  }

  //
  // Listeners

  onExpandPress = () => {
    this.setState({ isExpanded: !this.state.isExpanded });
  };

  //
  // Render

  render() {
    const {
      series,
      events,
      isDownloading,
      showEpisodeInformation,
      showFinaleIcon,
      timeFormat,
      fullColorEvents,
      colorImpairedMode,
      onEventModalOpenToggle
    } = this.props;

    const { isExpanded } = this.state;
    const {
      allDownloaded,
      anyQueued,
      anyMonitored,
      allAbsoluteEpisodeNumbers
    } = getEventsInfo(series, events);
    const anyDownloading = isDownloading || anyQueued;
    const firstEpisode = events[0];
    const lastEpisode = events[events.length -1];
    const airDateUtc = firstEpisode.airDateUtc;
    const startTime = moment(airDateUtc);
    const endTime = moment(lastEpisode.airDateUtc).add(series.runtime, 'minutes');
    const seasonNumber = firstEpisode.seasonNumber;
    const statusStyle = getStatusStyle(allDownloaded, anyDownloading, startTime, endTime, anyMonitored);
    const isMissingAbsoluteNumber = series.seriesType === 'anime' && seasonNumber > 0 && !allAbsoluteEpisodeNumbers;

    if (isExpanded) {
      return (
        <div>
          {
            events.map((event) => {
              if (event.isGroup) {
                return null;
              }

              return (
                <CalendarEventConnector
                  key={event.id}
                  episodeId={event.id}
                  {...event}
                  onEventModalOpenToggle={onEventModalOpenToggle}
                />
              );
            })
          }

          <Link
            className={styles.collapseContainer}
            component="div"
            onPress={this.onExpandPress}
          >
            <Icon
              name={icons.COLLAPSE}
            />
          </Link>
        </div>
      );
    }

    return (
      <div
        className={classNames(
          styles.eventGroup,
          styles[statusStyle],
          colorImpairedMode && 'colorImpaired',
          fullColorEvents && 'fullColor'
        )}
      >
        <div className={styles.info}>
          <div className={styles.seriesTitle}>
            {series.title}
          </div>

          {
            isMissingAbsoluteNumber &&
              <Icon
                containerClassName={styles.statusIcon}
                name={icons.WARNING}
                title={translate('EpisodeMissingAbsoluteNumber')}
              />
          }

          {
            anyDownloading &&
              <Icon
                containerClassName={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('AnEpisodeIsDownloading')}
              />
          }

          {
            firstEpisode.episodeNumber === 1 && seasonNumber > 0 &&
              <Icon
                containerClassName={styles.statusIcon}
                name={icons.INFO}
                kind={kinds.INFO}
                darken={fullColorEvents}
                title={seasonNumber === 1 ? translate('SeriesPremiere') : translate('SeasonPremiere')}
              />
          }

          {
            showFinaleIcon &&
            lastEpisode.finaleType ?
              <Icon
                containerClassName={styles.statusIcon}
                name={icons.INFO}
                kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
                title={getFinaleTypeName(lastEpisode.finaleType)}
              /> : null
          }
        </div>

        <div className={styles.airingInfo}>
          <div className={styles.airTime}>
            {formatTime(airDateUtc, timeFormat)} - {formatTime(endTime.toISOString(), timeFormat, { includeMinuteZero: true })}
          </div>

          {
            showEpisodeInformation ?
              <div className={styles.episodeInfo}>
                {seasonNumber}x{padNumber(firstEpisode.episodeNumber, 2)}-{padNumber(lastEpisode.episodeNumber, 2)}

                {
                  series.seriesType === 'anime' &&
                  firstEpisode.absoluteEpisodeNumber &&
                  lastEpisode.absoluteEpisodeNumber &&
                    <span className={styles.absoluteEpisodeNumber}>
                      ({firstEpisode.absoluteEpisodeNumber}-{lastEpisode.absoluteEpisodeNumber})
                    </span>
                }
              </div> :
              <Link
                className={styles.expandContainerInline}
                component="div"
                onPress={this.onExpandPress}
              >
                <Icon
                  name={icons.EXPAND}
                />
              </Link>
          }
        </div>

        {
          showEpisodeInformation &&
            <Link
              className={styles.expandContainer}
              component="div"
              onPress={this.onExpandPress}
            >
              <Icon
                name={icons.EXPAND}
              />
            </Link>
        }
      </div>
    );
  }
}

CalendarEventGroup.propTypes = {
  // Most of these props come from the connector and are required, but TS is confused.
  series: PropTypes.object,
  events: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDownloading: PropTypes.bool,
  showEpisodeInformation: PropTypes.bool,
  showFinaleIcon: PropTypes.bool,
  fullColorEvents: PropTypes.bool,
  timeFormat: PropTypes.string,
  colorImpairedMode: PropTypes.bool,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEventGroup;
