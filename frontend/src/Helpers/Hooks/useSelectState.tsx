import { cloneDeep } from 'lodash';
import { useReducer } from 'react';
import areAllSelected from 'Utilities/Table/areAllSelected';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';

export type SelectedState = Record<number | string, boolean>;

export interface SelectStateModel {
  id: number | string;
}

export interface SelectState {
  selectedState: SelectedState;
  lastToggled: number | string | null;
  allSelected: boolean;
  allUnselected: boolean;
}

export type SelectAction =
  | { type: 'reset' }
  | { type: 'selectAll'; items: SelectStateModel[] }
  | { type: 'unselectAll'; items: SelectStateModel[] }
  | {
      type: 'toggleSelected';
      id: number | string;
      isSelected: boolean | null;
      shiftKey: boolean;
      items: SelectStateModel[];
    }
  | {
      type: 'removeItem';
      id: number | string;
    }
  | {
      type: 'updateItems';
      items: SelectStateModel[];
    };

export type Dispatch = (action: SelectAction) => void;

const initialState = {
  selectedState: {},
  lastToggled: null,
  allSelected: false,
  allUnselected: true,
  items: [],
};

function getSelectedState(
  items: SelectStateModel[],
  existingState: SelectedState
) {
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
