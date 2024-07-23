import ModelBase from 'App/ModelBase';
import { SelectState } from 'Helpers/Hooks/useSelectState';
import areAllSelected from './areAllSelected';
import getToggledRange from './getToggledRange';

function toggleSelected<T extends ModelBase>(
  selectState: SelectState,
  items: T[],
  id: number,
  selected: boolean,
  shiftKey: boolean
) {
  const lastToggled = selectState.lastToggled;
  const nextSelectedState = {
    ...selectState.selectedState,
    [id]: selected,
  };

  if (selected == null) {
    delete nextSelectedState[id];
  }

  if (shiftKey && lastToggled) {
    const { lower, upper } = getToggledRange(items, id, lastToggled);

    for (let i = lower; i < upper; i++) {
      nextSelectedState[items[i].id] = selected;
    }
  }

  return {
    ...areAllSelected(nextSelectedState),
    lastToggled: id,
    selectedState: nextSelectedState,
  };
}

export default toggleSelected;
