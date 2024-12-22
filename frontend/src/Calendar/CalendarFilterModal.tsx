import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setCalendarFilter } from 'Store/Actions/calendarActions';

function createCalendarSelector() {
  return createSelector(
    (state: AppState) => state.calendar.items,
    (calendar) => {
      return calendar;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.calendar.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

type CalendarFilterModalProps = FilterModalProps<History>;

export default function CalendarFilterModal(props: CalendarFilterModalProps) {
  const sectionItems = useSelector(createCalendarSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'calendar';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setCalendarFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
