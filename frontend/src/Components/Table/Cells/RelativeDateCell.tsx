import React from 'react';
import { useSelector } from 'react-redux';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import TableRowCell from './TableRowCell';
import styles from './RelativeDateCell.css';

interface RelativeDateCellProps {
  className?: string;
  date?: string;
  includeSeconds?: boolean;
  includeTime?: boolean;
  component?: React.ElementType;
}

function RelativeDateCell(props: RelativeDateCellProps) {
  const {
    className = styles.cell,
    date,
    includeSeconds = false,
    includeTime = false,

    component: Component = TableRowCell,
    ...otherProps
  } = props;

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  if (!date) {
    return <Component className={className} {...otherProps} />;
  }

  return (
    <Component
      className={className}
      title={formatDateTime(date, longDateFormat, timeFormat, {
        includeSeconds,
        includeRelativeDay: !showRelativeDates,
      })}
      {...otherProps}
    >
      {getRelativeDate({
        date,
        shortDateFormat,
        showRelativeDates,
        timeFormat,
        includeSeconds,
        includeTime,
        timeForToday: true,
      })}
    </Component>
  );
}

export default RelativeDateCell;
