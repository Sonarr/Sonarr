import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { setCalendarView, gotoCalendarToday, gotoCalendarPreviousRange, gotoCalendarNextRange } from 'Store/Actions/calendarActions';
import CalendarHeader from './CalendarHeader';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createDimensionsSelector(),
    createUISettingsSelector(),
    (calendar, dimensions, uiSettings) => {
      const result = _.pick(calendar, [
        'isFetching',
        'view',
        'time',
        'start',
        'end'
      ]);

      result.isSmallScreen = dimensions.isSmallScreen;
      result.longDateFormat = uiSettings.longDateFormat;

      return result;
    }
  );
}

const mapDispatchToProps = {
  setCalendarView,
  gotoCalendarToday,
  gotoCalendarPreviousRange,
  gotoCalendarNextRange
};

class CalendarHeaderConnector extends Component {

  //
  // Listeners

  onViewChange = (view) => {
    this.props.setCalendarView({ view });
  }

  onTodayPress = () => {
    this.props.gotoCalendarToday();
  }

  onPreviousPress = () => {
    this.props.gotoCalendarPreviousRange();
  }

  onNextPress = () => {
    this.props.gotoCalendarNextRange();
  }

  //
  // Render

  render() {
    return (
      <CalendarHeader
        {...this.props}
        onViewChange={this.onViewChange}
        onTodayPress={this.onTodayPress}
        onPreviousPress={this.onPreviousPress}
        onNextPress={this.onNextPress}
      />
    );
  }
}

CalendarHeaderConnector.propTypes = {
  setCalendarView: PropTypes.func.isRequired,
  gotoCalendarToday: PropTypes.func.isRequired,
  gotoCalendarPreviousRange: PropTypes.func.isRequired,
  gotoCalendarNextRange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CalendarHeaderConnector);
