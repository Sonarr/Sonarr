import { FilterBuilderTag } from 'Components/Filter/Builder/FilterBuilderRowValue';
import { SelectedFilterKey } from 'Components/Filter/Filter';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import InteractiveSearchPayload from './InteractiveSearchPayload';

export interface ReleaseOptions {
  episodeSelectedFilterKey: SelectedFilterKey;
  seasonSelectedFilterKey: SelectedFilterKey;
  rejectionFilterTags: FilterBuilderTag<string, string>[];
}

const { useOption, getOptions, getOption, setOptions, setOption } =
  createOptionsStore<ReleaseOptions>(
    'release_options',
    () => {
      return {
        episodeSelectedFilterKey: 'all',
        seasonSelectedFilterKey: 'season-pack',
        rejectionFilterTags: [],
      };
    },
    {
      version: 1,
      migrate: (persistedState) => {
        const { columns: _columns, ...options } =
          persistedState as ReleaseOptions & { columns?: unknown };

        return options;
      },
    }
  );

export const getReleaseOptions = getOptions;
export const setReleaseOptions = setOptions;
export const useReleaseOption = useOption;
export const getReleaseOption = getOption;
export const setReleaseOption = setOption;

export const getSelectedFilterKeyOption = (payload: InteractiveSearchPayload) =>
  'seriesId' in payload
    ? ('seasonSelectedFilterKey' as const)
    : ('episodeSelectedFilterKey' as const);
