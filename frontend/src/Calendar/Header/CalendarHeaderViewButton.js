import PropTypes from 'prop-types';
import React, { Component } from 'react';
import * as calendarViews from 'Calendar/calendarViews';
import Button from 'Components/Link/Button';
import titleCase from 'Utilities/String/titleCase';
// import styles from './CalendarHeaderViewButton.css';

class CalendarHeaderViewButton extends Component {

  //
  // Listeners

  onPress = () => {
    this.props.onPress(this.props.view);
  };

  //
  // Render

  render() {
    const {
      view,
      selectedView,
      ...otherProps
    } = this.props;

    return (
      <Button
        isDisabled={selectedView === view}
        {...otherProps}
        onPress={this.onPress}
      >
        {titleCase(view)}
      </Button>
    );
  }
}

CalendarHeaderViewButton.propTypes = {
  view: PropTypes.oneOf(calendarViews.all).isRequired,
  selectedView: PropTypes.oneOf(calendarViews.all).isRequired,
  onPress: PropTypes.func.isRequired
};

export default CalendarHeaderViewButton;
