import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';
import ImportMode from './ImportMode';

export interface InteractiveImportOptions
  extends Omit<PageableOptions, 'pageSize'> {
  importMode: ImportMode;
}

export const COLUMNS = [
  {
    name: 'relativePath',
    label: 'Relative Path',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'series',
    label: 'Series',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'season',
    label: 'Season',
    isVisible: true,
  },
  {
    name: 'episodes',
    label: 'Episodes',
    isVisible: true,
  },
  {
    name: 'releaseGroup',
    label: 'Release Group',
    isVisible: true,
  },
  {
    name: 'quality',
    label: 'Quality',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languages',
    label: 'Languages',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'releaseType',
    label: 'Release Type',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: 'Custom Formats',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexerFlags',
    label: 'Indexer Flags',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: 'Rejections',
    isSortable: true,
    isVisible: true,
  },
];

function createInteractiveImportOptionsStore(
  name: string,
  sortDirection: SortDirection
) {
  return createOptionsStore<InteractiveImportOptions>(name, () => {
    return {
      selectedFilterKey: 'all',
      sortKey: 'relativePath',
      sortDirection,
      importMode: 'chooseImportMode',
      columns: COLUMNS,
    };
  });
}

export type InteractiveImportOptionsStore = ReturnType<
  typeof createInteractiveImportOptionsStore
>;

export const interactiveImportOptionsStore =
  createInteractiveImportOptionsStore(
    'interactive_import_options',
    'ascending'
  );

export const manageEpisodesOptionsStore = createInteractiveImportOptionsStore(
  'manage_episodes_options',
  'descending'
);
