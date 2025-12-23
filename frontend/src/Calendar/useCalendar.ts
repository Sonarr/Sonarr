import { keepPreviousData } from '@tanstack/react-query';
import moment from 'moment';
import { useEffect, useMemo } from 'react';
import { useSelector } from 'react-redux';
import { create } from 'zustand';
import AppState from 'App/State/AppState';
import { setEpisodeQueryKey } from 'Episode/useEpisode';
import { Filter, FilterBuilderProp } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { filterBuilderValueTypes } from 'Helpers/Props';
import { CalendarItem } from 'typings/Calendar';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import translate from 'Utilities/String/translate';
import { getCalendarOption, useCalendarOption } from './calendarOptionsStore';
import { CalendarView } from './calendarViews';

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'monitored',
    label: () => translate('MonitoredOnly'),
    filters: [
      {
        key: 'unmonitored',
        value: [false],
        type: 'equal',
      },
    ],
  },
];

export const FILTER_BUILDER: FilterBuilderProp<CalendarItem>[] = [
  {
    name: 'unmonitored',
    label: () => translate('IncludeUnmonitored'),
    type: 'equal',
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'includeSpecials',
    label: () => translate('IncludeSpecials'),
    type: 'equal',
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'tags',
    label: () => translate('Tags'),
    type: 'contains',
    valueType: filterBuilderValueTypes.TAG,
  },
];

interface CalendarStore {
  time: moment.Moment;
  dates: string[];
  dayCount: number;
  searchMissingCommandId?: number;
}

const calendarStore = create<CalendarStore>(() => ({
  time: moment(),
  dates: [],
  dayCount: 7,
  queryKey: null,
}));

const VIEW_RANGES: Record<
  CalendarView,
  moment.unitOfTime.DurationConstructor | undefined
> = {
  agenda: undefined,
  day: 'day',
  week: 'week',
  month: 'month',
  forecast: 'day',
};

const useCalendar = () => {
  const dates = useCalendarDates();
  const time = useCalendarTime();
  const selectedFilterKey = useCalendarOption('selectedFilterKey');
  const view = useCalendarOption('view');
  const customFilters = useCustomFiltersList('calendar');

  const { start, end } = useMemo(() => {
    return getPopulatableRange(dates[0], dates[dates.length - 1], view);
  }, [dates, view]);

  const { includeUnmonitored, includeSpecials, tags } = useMemo(() => {
    const selectedFilters = findSelectedFilters(
      selectedFilterKey,
      FILTERS,
      customFilters
    );

    return selectedFilters.reduce<{
      includeUnmonitored: boolean;
      includeSpecials: boolean;
      tags?: number[] | undefined;
    }>(
      (acc, filter) => {
        if (filter.key === 'unmonitored' && Array.isArray(filter.value)) {
          acc.includeUnmonitored = (filter.value as boolean[]).includes(true);
        }

        if (filter.key === 'includeSpecials' && Array.isArray(filter.value)) {
          acc.includeSpecials = (filter.value as boolean[]).includes(true);
        }

        if (filter.key === 'tags' && filter.type === 'contains') {
          acc.tags = filter.value as number[];
        }

        return acc;
      },
      {
        includeUnmonitored: false,
        includeSpecials: true,
      }
    );
  }, [customFilters, selectedFilterKey]);

  const { queryKey, ...result } = useApiQuery<CalendarItem[]>({
    path: '/calendar',
    queryParams: {
      start,
      end,
      includeUnmonitored,
      includeSpecials,
      tags,
    },
    queryOptions: {
      enabled: !!time && !!start && !!end,
      placeholderData: keepPreviousData,
    },
  });

  useEffect(() => {
    setEpisodeQueryKey('calendar', queryKey);
  }, [queryKey]);

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useCalendar;

export const useCalendarPage = () => {
  const dayCount = useCalendarDayCount();
  const time = useCalendarTime();
  const view = useCalendarOption('view');
  const firstDayOfWeek = useSelector(
    (state: AppState) => state.settings.ui.item.firstDayOfWeek
  );

  useEffect(() => {
    const { dates } = getDates(time, view, firstDayOfWeek, dayCount);

    calendarStore.setState({ dates });
  }, [firstDayOfWeek, dayCount, time, view]);
};

export const useCalendarTime = () => {
  return calendarStore((state) => state.time);
};

export const useCalendarDates = () => {
  return calendarStore((state) => state.dates);
};

export const useCalendarDayCount = () => {
  return calendarStore((state) => state.dayCount);
};

export const useCalendarRange = () => {
  const dates = useCalendarDates();

  return {
    start: dates[0],
    end: dates[dates.length - 1],
  };
};

export const useCalendarSearchMissingCommandId = () => {
  return calendarStore((state) => state.searchMissingCommandId);
};

export const setCalendarDayCount = (dayCount: number) => {
  calendarStore.setState({ dayCount });
};

export const goToToday = () => {
  setCalendarTime(moment());
};

export const goToPreviousRange = () => {
  const { dayCount, time } = calendarStore.getState();
  const view = getCalendarOption('view');

  const amount = view === 'forecast' ? dayCount : 1;
  const newTime = moment(time).subtract(amount, VIEW_RANGES[view]);

  setCalendarTime(newTime);
};

export const goToNextRange = () => {
  const { dayCount, time } = calendarStore.getState();
  const view = getCalendarOption('view');

  const amount = view === 'forecast' ? dayCount : 1;
  const newTime = moment(time).add(amount, VIEW_RANGES[view]);

  setCalendarTime(newTime);
};

const setCalendarTime = (time: moment.Moment) => {
  calendarStore.setState({ time });
};

const getDays = (start: moment.Moment, end: moment.Moment) => {
  const startTime = moment(start);
  const endTime = moment(end);
  const difference = endTime.diff(startTime, 'days');

  return Array(difference + 1)
    .fill(0)
    .map((_, i) => startTime.clone().add(i, 'days').toISOString());
};

const getDates = (
  time: moment.Moment,
  view: CalendarView,
  firstDayOfWeek: number,
  dayCount: number
) => {
  const weekName = firstDayOfWeek === 0 ? 'week' : 'isoWeek';

  let start = time.clone().startOf('day');
  let end = time.clone().endOf('day');

  if (view === 'week') {
    start = time.clone().startOf(weekName);
    end = time.clone().endOf(weekName);
  }

  if (view === 'forecast') {
    start = time.clone().subtract(1, 'day').startOf('day');
    end = time
      .clone()
      .add(dayCount - 2, 'days')
      .endOf('day');
  }

  if (view === 'month') {
    start = time.clone().startOf('month').startOf(weekName);
    end = time.clone().endOf('month').endOf(weekName);
  }

  if (view === 'agenda') {
    start = time.clone().subtract(1, 'day').startOf('day');
    end = time.clone().add(1, 'month').endOf('day');
  }

  return {
    start: start.toISOString(),
    end: end.toISOString(),
    time: time.toISOString(),
    dates: getDays(start, end),
  };
};

function getPopulatableRange(
  startDate: string,
  endDate: string,
  view: CalendarView
) {
  switch (view) {
    case 'day':
      return {
        start: moment(startDate).subtract(1, 'day').toISOString(),
        end: moment(endDate).add(1, 'day').toISOString(),
      };
    case 'week':
    case 'forecast':
      return {
        start: moment(startDate).subtract(1, 'week').toISOString(),
        end: moment(endDate).add(1, 'week').toISOString(),
      };
    default:
      return {
        start: startDate,
        end: endDate,
      };
  }
}
