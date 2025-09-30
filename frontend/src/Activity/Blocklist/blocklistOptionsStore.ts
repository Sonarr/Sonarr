import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

export type BlocklistOptions = PageableOptions;

const { useOptions, useOption, setOptions, setOption } =
  createOptionsStore<BlocklistOptions>('blocklist_options', () => {
    return {
      pageSize: 20,
      selectedFilterKey: 'all',
      sortKey: 'time',
      sortDirection: 'descending',
      columns: [
        {
          name: 'series.sortTitle',
          label: () => translate('SeriesTitle'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'sourceTitle',
          label: () => translate('SourceTitle'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'languages',
          label: () => translate('Languages'),
          isVisible: false,
        },
        {
          name: 'quality',
          label: () => translate('Quality'),
          isVisible: true,
        },
        {
          name: 'customFormats',
          label: () => translate('Formats'),
          isSortable: false,
          isVisible: true,
        },
        {
          name: 'date',
          label: () => translate('Date'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'indexer',
          label: () => translate('Indexer'),
          isSortable: true,
          isVisible: false,
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

export const useBlocklistOptions = useOptions;
export const setBlocklistOptions = setOptions;
export const useBlocklistOption = useOption;
export const setBlocklistOption = setOption;
