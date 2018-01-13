import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import CalendarDay from './CalendarDay';

function sort(items) {
  return _.sortBy(items, (item) => {
    if (item.isGroup) {
      return moment(item.events[0].airDateUtc).unix();
    }

    return moment(item.airDateUtc).unix();
  });
}

function createCalendarEventsConnector() {
  return createSelector(
    (state, { date }) => date,
    (state) => state.calendar.items,
    (state) => state.calendar.options.collapseMultipleEpisodes,
    (date, items, collapseMultipleEpisodes) => {
      const filtered = _.filter(items, (item) => {
        return moment(date).isSame(moment(item.airDateUtc), 'day');
      });

      if (!collapseMultipleEpisodes) {
        return sort(filtered);
      }

      const groupedObject = _.groupBy(filtered, (item) => `${item.seriesId}-${item.seasonNumber}`);
      const grouped = [];

      Object.keys(groupedObject).forEach((key) => {
        const events = groupedObject[key];

        if (events.length === 1) {
          grouped.push(events[0]);
        } else {
          grouped.push({
            isGroup: true,
            seriesId: events[0].seriesId,
            seasonNumber: events[0].seasonNumber,
            episodeIds: events.map((event) => event.id),
            events: _.sortBy(events, (item) => moment(item.airDateUtc).unix())
          });
        }
      });

      const sorted = sort(grouped);

      return sorted;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createCalendarEventsConnector(),
    (calendar, events) => {
      return {
        time: calendar.time,
        view: calendar.view,
        events
      };
    }
  );
}

class CalendarDayConnector extends Component {

  //
  // Render

  render() {
    return (
      <CalendarDay
        {...this.props}
      />
    );
  }
}

CalendarDayConnector.propTypes = {
  date: PropTypes.string.isRequired
};

export default connect(createMapStateToProps)(CalendarDayConnector);
