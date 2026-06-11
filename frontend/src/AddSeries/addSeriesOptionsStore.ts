import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SeriesEdition, SeriesMonitor, SeriesType } from 'Series/Series';

export interface AddSeriesOptions {
  rootFolderPath: string;
  monitor: SeriesMonitor;
  qualityProfileId: number;
  seriesEdition: SeriesEdition;
  seriesType: SeriesType;
  seasonFolder: boolean;
  searchForMissingEpisodes: boolean;
  searchForCutoffUnmetEpisodes: boolean;
  tags: number[];
}

const { useOptions, useOption, setOption } =
  createOptionsStore<AddSeriesOptions>('add_series_options', () => {
    return {
      rootFolderPath: '',
      monitor: 'all',
      qualityProfileId: 0,
      seriesEdition: 'standard',
      seriesType: 'standard',
      seasonFolder: true,
      searchForMissingEpisodes: false,
      searchForCutoffUnmetEpisodes: false,
      tags: [],
    };
  });

export const useAddSeriesOptions = useOptions;
export const useAddSeriesOption = useOption;
export const setAddSeriesOption = setOption;
