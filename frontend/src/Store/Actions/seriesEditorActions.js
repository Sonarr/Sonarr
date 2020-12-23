import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import sortByName from 'Utilities/Array/sortByName'
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, updateItem } from './baseActions';
import { filters, filterPredicates, sortPredicates } from './seriesActions';

//
// Variables

export const section = 'seriesEditor';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  selectedFilterKey: 'all',
  filters,
  filterPredicates: {
    ...filterPredicates,

    episodeProgress: function(item, filterValue, type) {
      const { statistics = {} } = item;

      const {
        episodeCount = 0,
        episodeFileCount
      } = statistics;

      const progress = episodeCount ?
        episodeFileCount / episodeCount * 100 :
        100;

      const predicate = filterTypePredicates[type];

      return predicate(progress, filterValue);
    }
  },

  columns: [
    {
      name: 'status',
      columnLabel: 'Status',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: 'Series Title',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'languageProfileId',
      label: 'Language Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seriesType',
      label: 'Type',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seasonFolder',
      label: 'Season Folder',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'tags',
      label: 'Tags',
      isSortable: true,
      isVisible: true
    }
  ],

  filterBuilderProps: [
    {
      name: 'monitored',
      label: 'Monitored',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'status',
      label: 'Status',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.SERIES_STATUS
    },
    {
      name: 'seriesType',
      label: 'Type',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.SERIES_TYPES
    },
    {
      name: 'network',
      label: 'Network',
      type: filterBuilderTypes.STRING,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, series) => {
          if (series.network) {
            acc.push({
              id: series.network,
              name: series.network
            });
          }

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'languageProfileId',
      label: 'Language Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.LANGUAGE_PROFILE
    },
    {
      name: 'nextAiring',
      label: 'Next Airing',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'previousAiring',
      label: 'Previous Airing',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'added',
      label: 'Added',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'seasonCount',
      label: 'Season Count',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'episodeProgress',
      label: 'Episode Progress',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'path',
      label: 'Path',
      type: filterBuilderTypes.STRING
    },
    {
      name: 'rootFolderPath',
      label: 'Root Folder Path',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.BYTES
    },
    {
      name: 'genres',
      label: 'Genres',
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, series) => {
          series.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre
            });
          });

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'ratings',
      label: 'Rating',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'certification',
      label: 'Certification',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'tags',
      label: 'Tags',
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    },
    {
      name: 'useSceneNumbering',
      label: 'Scene Numbering',
      type: filterBuilderTypes.EXACT
    }
  ],

  sortPredicates
};

export const persistState = [
  'seriesEditor.sortKey',
  'seriesEditor.sortDirection',
  'seriesEditor.selectedFilterKey',
  'seriesEditor.customFilters'
];


//
// Actions Types

export const SET_SERIES_EDITOR_SORT = 'seriesEditor/setSeriesEditorSort';
export const SET_SERIES_EDITOR_FILTER = 'seriesEditor/setSeriesEditorFilter';
export const SAVE_SERIES_EDITOR = 'seriesEditor/saveSeriesEditor';
export const BULK_DELETE_SERIES = 'seriesEditor/bulkDeleteSeries';
export const SET_SERIES_EDITOR_TABLE_OPTION = 'seriesIndex/setSeriesEditorTableOption';

//
// Action Creators

export const setSeriesEditorSort = createAction(SET_SERIES_EDITOR_SORT);
export const setSeriesEditorFilter = createAction(SET_SERIES_EDITOR_FILTER);
export const saveSeriesEditor = createThunk(SAVE_SERIES_EDITOR);
export const bulkDeleteSeries = createThunk(BULK_DELETE_SERIES);
export const setSeriesEditorTableOption = createAction(SET_SERIES_EDITOR_TABLE_OPTION);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [SAVE_SERIES_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((series) => {
          return updateItem({
            id: series.id,
            section: 'series',
            ...series
          });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  },

  [BULK_DELETE_SERIES]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isDeleting: true
    }));

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done(() => {
      // SignaR will take care of removing the series from the collection

      dispatch(set({
        section,
        isDeleting: false,
        deleteError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_EDITOR_TABLE_OPTION]: createSetTableOptionReducer(section),
  [SET_SERIES_EDITOR_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SERIES_EDITOR_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);
