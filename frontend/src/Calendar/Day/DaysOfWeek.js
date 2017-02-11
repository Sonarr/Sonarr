import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DayOfWeek from './DayOfWeek';
import * as calendarViews from 'Calendar/calendarViews';
import styles from './DaysOfWeek.css';

class DaysOfWeek extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      todaysDate: moment().startOf('day').toISOString()
    };

    this.updateTimeoutId = null;
  }

  // Lifecycle

  componentDidMount() {
    const view = this.props.view;

    if (view !== calendarViews.AGENDA || view !== calendarViews.MONTH) {
      this.scheduleUpdate();
    }
  }

  componentWillUnmount() {
    this.clearUpdateTimeout();
  }

  //
  // Control

  scheduleUpdate = () => {
    this.clearUpdateTimeout();
    const todaysDate = moment().startOf('day');
    const diff = todaysDate.clone().add(1, 'day').diff(moment());

    this.setState({
      todaysDate: todaysDate.toISOString()
    });

    this.updateTimeoutId = setTimeout(this.scheduleUpdate, diff);
  }

  clearUpdateTimeout = () => {
    if (this.updateTimeoutId) {
      clearTimeout(this.updateTimeoutId);
    }
  }

  //
  // Render

  render() {
    const {
      dates,
      view,
      ...otherProps
    } = this.props;

    if (view === calendarViews.AGENDA) {
      return null;
    }

    return (
      <div className={styles.daysOfWeek}>
        {
          dates.map((date) => {
            return (
              <DayOfWeek
                key={date}
                date={date}
                view={view}
                isTodaysDate={date === this.state.todaysDate}
                {...otherProps}
              />
            );
          })
        }
      </div>
    );
  }
}

DaysOfWeek.propTypes = {
  dates: PropTypes.arrayOf(PropTypes.string),
  view: PropTypes.string.isRequired
};

export default DaysOfWeek;
