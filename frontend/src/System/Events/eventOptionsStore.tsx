import Column from 'Components/Table/Column';
import { createPersist, mergeColumns } from 'Helpers/createPersist';
import { SortDirection } from 'Helpers/Props/sortDirections';
import translate from 'Utilities/String/translate';

export interface EventOptions {
  pageSize: number;
  selectedFilterKey: string | number;
  sortKey: string;
  sortDirection: SortDirection;
  columns: Column[];
}

const eventOptionsStore = createPersist<EventOptions>(
  'event_options',
  () => {
    return {
      pageSize: 50,
      selectedFilterKey: 'all',
      sortKey: 'time',
      sortDirection: 'descending',
      columns: [
        {
          name: 'level',
          label: '',
          columnLabel: () => translate('Level'),
          isSortable: false,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'time',
          label: () => translate('Time'),
          isSortable: true,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'logger',
          label: () => translate('Component'),
          isSortable: false,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'message',
          label: () => translate('Message'),
          isVisible: true,
          isModifiable: false,
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
  },
  {
    merge: mergeColumns,
  }
);

export const useEventOptions = () => {
  return eventOptionsStore((state) => state);
};

export const setEventOptions = (options: Partial<EventOptions>) => {
  eventOptionsStore.setState((state) => ({
    ...state,
    ...options,
  }));
};

export const setEventOption = <K extends keyof EventOptions>(
  key: K,
  value: EventOptions[K]
) => {
  eventOptionsStore.setState((state) => ({
    ...state,
    [key]: value,
  }));
};
