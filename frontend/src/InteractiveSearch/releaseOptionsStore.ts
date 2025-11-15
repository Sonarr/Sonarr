import { createElement } from 'react';
import { FilterBuilderTag } from 'Components/Filter/Builder/FilterBuilderRowValue';
import { SelectedFilterKey } from 'Components/Filter/Filter';
import Icon from 'Components/Icon';
import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export interface ReleaseOptions
  extends Omit<
    PageableOptions,
    'pageSize' | 'selectedFilterKey' | 'sortKey' | 'sortDirection'
  > {
  episodeSelectedFilterKey: SelectedFilterKey;
  seasonSelectedFilterKey: SelectedFilterKey;
  rejectionFilterTags: FilterBuilderTag<string, string>[];
}

const { useOptions, useOption, getOptions, getOption, setOptions, setOption } =
  createOptionsStore<ReleaseOptions>('release_options', () => {
    return {
      episodeSelectedFilterKey: 'all',
      seasonSelectedFilterKey: 'season-pack',
      rejectionFilterTags: [],
      columns: [
        {
          name: 'protocol',
          label: () => translate('Source'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'age',
          label: () => translate('Age'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'title',
          label: () => translate('Title'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'indexer',
          label: () => translate('Indexer'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'size',
          label: () => translate('Size'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'peers',
          label: () => translate('Peers'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'languages',
          label: () => translate('Languages'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'qualityWeight',
          label: () => translate('Quality'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'customFormatScore',
          label: createElement(Icon, {
            name: icons.SCORE,
            title: () => translate('CustomFormatScore'),
          }),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'indexerFlags',
          label: createElement(Icon, {
            name: icons.FLAG,
            title: () => translate('IndexerFlags'),
          }),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'rejections',
          label: createElement(Icon, {
            name: icons.DANGER,
            title: () => translate('Rejections'),
          }),
          isSortable: true,
          fixedSortDirection: 'ascending',
          isVisible: true,
        },
        {
          name: 'releaseWeight',
          label: createElement(Icon, { name: icons.DOWNLOAD }),
          isSortable: true,
          fixedSortDirection: 'ascending',
          isVisible: true,
        },
      ],
    };
  });

export const useReleaseOptions = useOptions;
export const getReleaseOptions = getOptions;
export const setReleaseOptions = setOptions;
export const useReleaseOption = useOption;
export const getReleaseOption = getOption;
export const setReleaseOption = setOption;
