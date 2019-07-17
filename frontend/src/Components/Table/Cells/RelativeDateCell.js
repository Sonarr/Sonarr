import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import TableRowCell from './TableRowCell';
import styles from './RelativeDateCell.css';

class RelativeDateCell extends PureComponent {

  //
  // Render

  render() {
    const {
      className,
      date,
      includeSeconds,
      showRelativeDates,
      shortDateFormat,
      longDateFormat,
      timeFormat,
      component: Component,
      dispatch,
      ...otherProps
    } = this.props;

    if (!date) {
      return (
        <Component
          className={className}
          {...otherProps}
        />
      );
    }

    return (
      <Component
        className={className}
        title={formatDateTime(date, longDateFormat, timeFormat, { includeSeconds, includeRelativeDay: !showRelativeDates })}
        {...otherProps}
      >
        {getRelativeDate(date, shortDateFormat, showRelativeDates, { timeFormat, includeSeconds, timeForToday: true })}
      </Component>
    );
  }
}

RelativeDateCell.propTypes = {
  className: PropTypes.string.isRequired,
  date: PropTypes.string,
  includeSeconds: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  component: PropTypes.elementType,
  dispatch: PropTypes.func
};

RelativeDateCell.defaultProps = {
  className: styles.cell,
  includeSeconds: false,
  component: TableRowCell
};

export default RelativeDateCell;
