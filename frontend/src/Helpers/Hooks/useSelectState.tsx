import { cloneDeep } from 'lodash';
import { useReducer } from 'react';
import ModelBase from 'App/ModelBase';
import areAllSelected from 'Utilities/Table/areAllSelected';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';

export type SelectedState = Record<number, boolean>;

export interface SelectState {
  selectedState: SelectedState;
  lastToggled: number | null;
  allSelected: boolean;
  allUnselected: boolean;
}

export type SelectAction =
  | { type: 'reset' }
  | { type: 'selectAll'; items: ModelBase[] }
  | { type: 'unselectAll'; items: ModelBase[] }
  | {
      type: 'toggleSelected';
      id: number;
      isSelected: boolean;
      shiftKey: boolean;
      items: ModelBase[];
    }
  | {
      type: 'removeItem';
      id: number;
    }
  | {
      type: 'updateItems';
      items: ModelBase[];
    };

export type Dispatch = (action: SelectAction) => void;

const initialState = {
  selectedState: {},
  lastToggled: null,
  allSelected: false,
  allUnselected: true,
  items: [],
};

function getSelectedState(items: ModelBase[], existingState: SelectedState) {
  return items.reduce((acc: SelectedState, item) => {
    const id = item.id;

    acc[id] = existingState[id] ?? false;

    return acc;
  }, {});
}

function selectReducer(state: SelectState, action: SelectAction): SelectState {
  const { selectedState } = state;

  switch (action.type) {
    case 'reset': {
      return cloneDeep(initialState);
    }
    case 'selectAll': {
      return {
        ...selectAll(selectedState, true),
      };
    }
    case 'unselectAll': {
      return {
        ...selectAll(selectedState, false),
      };
    }
    case 'toggleSelected': {
      const result = {
        ...toggleSelected(
          state,
          action.items,
          action.id,
          action.isSelected,
          action.shiftKey
        ),
      };

      return result;
    }
    case 'updateItems': {
      const nextSelectedState = getSelectedState(action.items, selectedState);

      return {
        ...state,
        ...areAllSelected(nextSelectedState),
        selectedState: nextSelectedState,
      };
    }
    default: {
      throw new Error(`Unhandled action type: ${action.type}`);
    }
  }
}

export default function useSelectState(): [SelectState, Dispatch] {
  const selectedState = getSelectedState([], {});

  const [state, dispatch] = useReducer(selectReducer, {
    selectedState,
    lastToggled: null,
    allSelected: false,
    allUnselected: true,
  });

  return [state, dispatch];
}
