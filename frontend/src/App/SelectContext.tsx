import { cloneDeep } from 'lodash';
import React, { useEffect } from 'react';
import areAllSelected from 'Utilities/Table/areAllSelected';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import ModelBase from './ModelBase';

export enum SelectActionType {
  Reset,
  SelectAll,
  UnselectAll,
  ToggleSelected,
  RemoveItem,
  UpdateItems,
}

type SelectedState = Record<number, boolean>;

interface SelectState {
  selectedState: SelectedState;
  lastToggled: number | null;
  allSelected: boolean;
  allUnselected: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  items: any[];
}

type SelectAction =
  | { type: SelectActionType.Reset }
  | { type: SelectActionType.SelectAll }
  | { type: SelectActionType.UnselectAll }
  | {
      type: SelectActionType.ToggleSelected;
      id: number;
      isSelected: boolean;
      shiftKey: boolean;
    }
  | {
      type: SelectActionType.RemoveItem;
      id: number;
    }
  | {
      type: SelectActionType.UpdateItems;
      items: ModelBase[];
    };

type Dispatch = (action: SelectAction) => void;

const initialState = {
  selectedState: {},
  lastToggled: null,
  allSelected: false,
  allUnselected: true,
  items: [],
};

interface SelectProviderOptions<T extends ModelBase> {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  children: any;
  items: Array<T>;
}

function getSelectedState(items: ModelBase[], existingState: SelectedState) {
  return items.reduce((acc: SelectedState, item) => {
    const id = item.id;

    acc[id] = existingState[id] ?? false;

    return acc;
  }, {});
}

// TODO: Can this be reused?

const SelectContext = React.createContext<[SelectState, Dispatch] | undefined>(
  cloneDeep(undefined)
);

function selectReducer(state: SelectState, action: SelectAction): SelectState {
  const { items, selectedState } = state;

  switch (action.type) {
    case SelectActionType.Reset: {
      return cloneDeep(initialState);
    }
    case SelectActionType.SelectAll: {
      return {
        items,
        ...selectAll(selectedState, true),
      };
    }
    case SelectActionType.UnselectAll: {
      return {
        items,
        ...selectAll(selectedState, false),
      };
    }
    case SelectActionType.ToggleSelected: {
      const result = {
        items,
        ...toggleSelected(
          state,
          items,
          action.id,
          action.isSelected,
          action.shiftKey
        ),
      };

      return result;
    }
    case SelectActionType.UpdateItems: {
      const nextSelectedState = getSelectedState(action.items, selectedState);

      return {
        ...state,
        ...areAllSelected(nextSelectedState),
        selectedState: nextSelectedState,
        items,
      };
    }
    default: {
      throw new Error(`Unhandled action type: ${action.type}`);
    }
  }
}

export function SelectProvider<T extends ModelBase>(
  props: SelectProviderOptions<T>
) {
  const { items } = props;
  const selectedState = getSelectedState(items, {});

  const [state, dispatch] = React.useReducer(selectReducer, {
    selectedState,
    lastToggled: null,
    allSelected: false,
    allUnselected: true,
    items,
  });

  const value: [SelectState, Dispatch] = [state, dispatch];

  useEffect(() => {
    dispatch({ type: SelectActionType.UpdateItems, items });
  }, [items]);

  return (
    <SelectContext.Provider value={value}>
      {props.children}
    </SelectContext.Provider>
  );
}

export function useSelect() {
  const context = React.useContext(SelectContext);

  if (context === undefined) {
    throw new Error('useSelect must be used within a SelectProvider');
  }

  return context;
}
