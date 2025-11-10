import React, { useCallback } from 'react';
import { SetFilter } from 'Components/Filter/Filter';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setCalendarOption } from './calendarOptionsStore';
import useCalendar, { FILTER_BUILDER } from './useCalendar';

type CalendarFilterModalProps = FilterModalProps<History>;

export default function CalendarFilterModal(props: CalendarFilterModalProps) {
  const { data } = useCalendar();
  const customFilterType = 'calendar';

  const dispatchSetFilter = useCallback(({ selectedFilterKey }: SetFilter) => {
    setCalendarOption('selectedFilterKey', selectedFilterKey);
  }, []);

  return (
    <FilterModal
      {...props}
      sectionItems={data}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
