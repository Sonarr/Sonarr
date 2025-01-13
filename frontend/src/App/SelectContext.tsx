import { cloneDeep } from 'lodash';
import React, { useCallback, useEffect } from 'react';
import useSelectState, {
  SelectState,
  SelectStateModel,
} from 'Helpers/Hooks/useSelectState';
import ModelBase from './ModelBase';

export type SelectContextAction =
  | { type: 'reset' }
  | { type: 'selectAll' }
  | { type: 'unselectAll' }
  | {
      type: 'toggleSelected';
      id: number | string;
      isSelected: boolean | null;
      shiftKey: boolean;
    }
  | {
      type: 'removeItem';
      id: number | string;
    }
  | {
      type: 'updateItems';
      items: ModelBase[];
    };

export type SelectDispatch = (action: SelectContextAction) => void;

interface SelectProviderOptions<T extends SelectStateModel> {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  children: any;
  items: Array<T>;
}

const SelectContext = React.createContext<
  [SelectState, SelectDispatch] | undefined
>(cloneDeep(undefined));

export function SelectProvider<T extends SelectStateModel>(
  props: SelectProviderOptions<T>
) {
  const { items } = props;
  const [state, dispatch] = useSelectState();

  const dispatchWrapper = useCallback(
    (action: SelectContextAction) => {
      switch (action.type) {
        case 'reset':
        case 'removeItem':
          dispatch(action);
          break;

        default:
          dispatch({
            ...action,
            items,
          });
          break;
      }
    },
    [items, dispatch]
  );

  const value: [SelectState, SelectDispatch] = [state, dispatchWrapper];

  useEffect(() => {
    dispatch({ type: 'updateItems', items });
  }, [items, dispatch]);

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
