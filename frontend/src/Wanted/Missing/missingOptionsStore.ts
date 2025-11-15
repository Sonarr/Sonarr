import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<PageableOptions>('missing_options', () => {
    return {
      pageSize: 20,
      selectedFilterKey: 'monitored',
      sortKey: 'episodes.airDateUtc',
      sortDirection: 'descending',
      columns: [
        {
          name: 'series.sortTitle',
          label: () => translate('SeriesTitle'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'episode',
          label: () => translate('Episode'),
          isVisible: true,
        },
        {
          name: 'episodes.title',
          label: () => translate('EpisodeTitle'),
          isVisible: true,
        },
        {
          name: 'episodes.airDateUtc',
          label: () => translate('AirDate'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'episodes.lastSearchTime',
          label: () => translate('LastSearched'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'status',
          label: () => translate('Status'),
          isVisible: true,
        },
        {
          name: 'actions',
          label: '',
          columnLabel: () => translate('Actions'),
          isVisible: true,
          isModifiable: false,
        },
      ],
    };
  });

export const useMissingOptions = useOptions;
export const setMissingOptions = setOptions;
export const useMissingOption = useOption;
export const setMissingOption = setOption;
export const setMissingSort = setSort;
