import { createPersist } from 'Helpers/createPersist';
import { SeriesMonitor, SeriesType } from 'Series/Series';

export interface AddSeriesOptions {
  rootFolderPath: string;
  monitor: SeriesMonitor;
  qualityProfileId: number;
  seriesType: SeriesType;
  seasonFolder: boolean;
  searchForMissingEpisodes: boolean;
  searchForCutoffUnmetEpisodes: boolean;
  tags: number[];
}

const addSeriesOptionsStore = createPersist<AddSeriesOptions>(
  'add_series_options',
  () => {
    return {
      rootFolderPath: '',
      monitor: 'all',
      qualityProfileId: 0,
      seriesType: 'standard',
      seasonFolder: true,
      searchForMissingEpisodes: false,
      searchForCutoffUnmetEpisodes: false,
      tags: [],
    };
  }
);

export const useAddSeriesOptions = () => {
  return addSeriesOptionsStore((state) => state);
};

export const useAddSeriesOption = <K extends keyof AddSeriesOptions>(
  key: K
) => {
  return addSeriesOptionsStore((state) => state[key]);
};

export const setAddSeriesOption = <K extends keyof AddSeriesOptions>(
  key: K,
  value: AddSeriesOptions[K]
) => {
  addSeriesOptionsStore.setState((state) => ({
    ...state,
    [key]: value,
  }));
};
