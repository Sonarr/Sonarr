import {
  createOptionsStore,
  PageableOptions,
} from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

export type EventOptions = PageableOptions;

const { useOptions, setOptions, setOption } = createOptionsStore<EventOptions>(
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
  }
);

export const useEventOptions = useOptions;
export const setEventOptions = setOptions;
export const setEventOption = setOption;
