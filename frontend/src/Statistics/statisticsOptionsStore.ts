import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';

interface StatisticsOptions {
  selectedFilterKey: string | number;
}

const { useOption, setOption } = createOptionsStore<StatisticsOptions>(
  'statistics_options',
  () => {
    return {
      selectedFilterKey: 'all',
    };
  }
);

export const useStatisticsOption = useOption;
export const setStatisticsOption = setOption;
