import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import * as calendarViews from 'Calendar/calendarViews';
import isToday from 'Utilities/Date/isToday';
import CalendarDayConnector from './CalendarDayConnector';
import styles from './CalendarDays.css';

class CalendarDays extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._touchStart = null;

    this.state = {
      todaysDate: moment().startOf('day').toISOString(),
      isEventModalOpen: false
    };

    this.updateTimeoutId = null;
  }

  // Lifecycle

  componentDidMount() {
    const view = this.props.view;

    if (view === calendarViews.MONTH) {
      this.scheduleUpdate();
    }

    window.addEventListener('touchstart', this.onTouchStart);
    window.addEventListener('touchend', this.onTouchEnd);
    window.addEventListener('touchcancel', this.onTouchCancel);
    window.addEventListener('touchmove', this.onTouchMove);
  }

  componentWillUnmount() {
    this.clearUpdateTimeout();

    window.removeEventListener('touchstart', this.onTouchStart);
    window.removeEventListener('touchend', this.onTouchEnd);
    window.removeEventListener('touchcancel', this.onTouchCancel);
    window.removeEventListener('touchmove', this.onTouchMove);
  }

  //
  // Control

  scheduleUpdate = () => {
    this.clearUpdateTimeout();
    const todaysDate = moment().startOf('day');
    const diff = moment().diff(todaysDate.clone().add(1, 'day'));

    this.setState({ todaysDate: todaysDate.toISOString() });

    this.updateTimeoutId = setTimeout(this.scheduleUpdate, diff);
  };

  clearUpdateTimeout = () => {
    if (this.updateTimeoutId) {
      clearTimeout(this.updateTimeoutId);
    }
  };

  //
  // Listeners

  onEventModalOpenToggle = (isEventModalOpen) => {
    this.setState({ isEventModalOpen });
  };

  onTouchStart = (event) => {
    const touches = event.touches;
    const touchStart = touches[0].pageX;

    if (touches.length !== 1) {
      return;
    }

    if (
      touchStart < 50 ||
      this.props.isSidebarVisible ||
      this.state.isEventModalOpen
    ) {
      return;
    }

    this._touchStart = touchStart;
  };

  onTouchEnd = (event) => {
    const touches = event.changedTouches;
    const currentTouch = touches[0].pageX;

    if (!this._touchStart) {
      return;
    }

    if (currentTouch > this._touchStart && currentTouch - this._touchStart > 100) {
      this.props.onNavigatePrevious();
    } else if (currentTouch < this._touchStart && this._touchStart - currentTouch > 100) {
      this.props.onNavigateNext();
    }

    this._touchStart = null;
  };

  onTouchCancel = (event) => {
    this._touchStart = null;
  };

  onTouchMove = (event) => {
    if (!this._touchStart) {
      return;
    }
  };

  //
  // Render

  render() {
    const {
      dates,
      view
    } = this.props;

    return (
      <div className={classNames(
        styles.days,
        styles[view]
      )}
      >
        {
          dates.map((date) => {
            return (
              <CalendarDayConnector
                key={date}
                date={date}
                isTodaysDate={isToday(date)}
                onEventModalOpenToggle={this.onEventModalOpenToggle}
              />
            );
          })
        }
      </div>
    );
  }
}

CalendarDays.propTypes = {
  dates: PropTypes.arrayOf(PropTypes.string).isRequired,
  view: PropTypes.string.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  onNavigatePrevious: PropTypes.func.isRequired,
  onNavigateNext: PropTypes.func.isRequired
};

export default CalendarDays;
